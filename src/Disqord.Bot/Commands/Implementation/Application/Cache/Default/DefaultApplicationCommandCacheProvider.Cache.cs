using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Qommon;

namespace Disqord.Bot.Commands.Application.Default;

public partial class DefaultApplicationCommandCacheProvider
{
    public class Cache : IApplicationCommandCache
    {
        public DefaultApplicationCommandCacheProvider Provider { get; }

        public bool CacheFileExists { get; }

        public MemoryStream MemoryStream { get; }

        public CacheJsonModel Model { get; }

        public bool HasChanges { get; protected set; }

        public virtual IReadOnlyCollection<Snowflake> GuildIds => Model.GuildCommands?.Keys ?? (IReadOnlyCollection<Snowflake>) Array.Empty<Snowflake>();

        public Cache(DefaultApplicationCommandCacheProvider provider, bool cacheFileExists, MemoryStream memoryStream, CacheJsonModel model)
        {
            Provider = provider;
            CacheFileExists = cacheFileExists;
            MemoryStream = memoryStream;
            Model = model;

            HasChanges = !cacheFileExists;
        }

        protected virtual Changes CreateChanges(bool areInitial)
            => new(areInitial);

        public virtual void Migrate()
        {
            var model = Model;
            if (model.SchemaVersion == 0)
            {
                model.SchemaVersion = SchemaVersion;
                return;
            }

            if (model.SchemaVersion == SchemaVersion)
                return;

            if (model.SchemaVersion > SchemaVersion)
            {
                throw new InvalidOperationException($"Unsupported cache schema version '{model.SchemaVersion}'. Current schema version is {SchemaVersion}.");
            }

            do
            {
                switch (model.SchemaVersion)
                {
                    case 0:
                    {
                        // Uninitialized model - set current schema version.
                        model.SchemaVersion = SchemaVersion;
                        return;
                    }
                    case SchemaVersion:
                    {
                        // Current schema version.
                        break;
                    }
                    default:
                    {
                        throw new InvalidOperationException($"Unsupported cache schema version '{model.SchemaVersion}'. Current schema version is {SchemaVersion}");
                    }
                }

                model.SchemaVersion++;
            }
            while (model.SchemaVersion < SchemaVersion);
        }

        public virtual IApplicationCommandCacheChanges GetChanges(Snowflake? guildId, IEnumerable<LocalApplicationCommand> commands)
        {
            var modelCommands = guildId == null
                ? Model.GlobalCommands
                : Model.GuildCommands?.GetValueOrDefault(guildId.Value);

            var changes = CreateChanges(!CacheFileExists || modelCommands == null);
            if (modelCommands == null)
            {
                changes.CreatedCommands.AddRange(commands);
                return changes;
            }

            var deletedCommandModels = new List<CommandJsonModel>(modelCommands);
            foreach (var modelCommand in modelCommands)
            {
                modelCommand.SetSerializer(Provider.Serializer);
            }

            foreach (var command in commands)
            {
                var commandType = CommandJsonModel.GetCommandType(command);
                CommandJsonModel? matchingModelCommand = null;
                foreach (var modelCommand in modelCommands)
                {
                    if (modelCommand.Name != command.Name || modelCommand.Type != commandType)
                        continue;

                    matchingModelCommand = modelCommand;
                    deletedCommandModels.Remove(modelCommand);
                    break;
                }

                if (matchingModelCommand == null)
                {
                    changes.CreatedCommands.Add(command);
                }
                else
                {
                    if (matchingModelCommand.Equals(command))
                    {
                        changes.UnchangedCommands.Add(command);
                    }
                    else
                    {
                        changes.ModifiedCommands.Add(matchingModelCommand.Id, command);
                    }
                }
            }

            var deletedCommandModelCount = deletedCommandModels.Count;
            for (var i = 0; i < deletedCommandModelCount; i++)
            {
                var deletedCommandModel = deletedCommandModels[i];
                changes.DeletedCommandIds.Add(deletedCommandModel.Id);
            }

            return changes;
        }

        public virtual void ApplyChanges(Snowflake? guildId, IApplicationCommandCacheChanges changes, IEnumerable<IApplicationCommand> commands)
        {
            var fastChanges = Guard.IsAssignableToType<Changes>(changes);

            if (!HasChanges)
                HasChanges = changes.Any;

            var modelCommands = guildId == null
                ? Model.GlobalCommands
                : Model.GuildCommands?.GetValueOrDefault(guildId.Value);

            var commandIds = commands.ToDictionary(x => (x.Name, x.Type), x => x.Id);
            var newModelCommands = new List<CommandJsonModel>((modelCommands?.Length ?? 0) + fastChanges.CreatedCommands.Count - fastChanges.DeletedCommandIds.Count);

            foreach (var unchangedCommand in fastChanges.UnchangedCommands)
            {
                var unchangedCommandType = CommandJsonModel.GetCommandType(unchangedCommand);
                CommandJsonModel existingModelCommand = null!;
                if (modelCommands != null)
                {
                    foreach (var modelCommand in modelCommands)
                    {
                        if (unchangedCommand.Name != modelCommand.Name || modelCommand.Type != unchangedCommandType)
                            continue;

                        existingModelCommand = modelCommand;
                        break;
                    }
                }

                if (commandIds.TryGetValue((unchangedCommand.Name.Value, unchangedCommandType), out var unchangedCommandId))
                {
                    existingModelCommand.Id = unchangedCommandId;
                }

                newModelCommands.Add(existingModelCommand);
            }

            foreach (var (modifiedCommandId, modifiedCommand) in fastChanges.ModifiedCommands)
            {
                CommandJsonModel existingModelCommand = null!;
                if (modelCommands != null)
                {
                    foreach (var modelCommand in modelCommands)
                    {
                        if (modifiedCommandId != modelCommand.Id)
                            continue;

                        existingModelCommand = modelCommand;
                        break;
                    }
                }

                existingModelCommand.Populate(modifiedCommand, Provider.Serializer);

                var modifiedCommandType = CommandJsonModel.GetCommandType(modifiedCommand);
                if (commandIds.TryGetValue((modifiedCommand.Name.Value, modifiedCommandType), out var refreshedModifiedCommandId))
                {
                    existingModelCommand.Id = refreshedModifiedCommandId;
                }

                newModelCommands.Add(existingModelCommand);
            }

            foreach (var createdCommand in fastChanges.CreatedCommands)
            {
                var newModelCommand = new CommandJsonModel(createdCommand, Provider.Serializer);
                newModelCommand.Id = commandIds[(createdCommand.Name.Value, CommandJsonModel.GetCommandType(createdCommand))];
                newModelCommands.Add(newModelCommand);
            }

            modelCommands = newModelCommands.ToArray();
            if (guildId == null)
            {
                Model.GlobalCommands = modelCommands;
            }
            else if (modelCommands.Length == 0)
            {
                Model.GuildCommands?.Remove(guildId.Value);
            }
            else
            {
                (Model.GuildCommands ??= new())[guildId.Value] = modelCommands;
            }
        }

        public ValueTask DisposeAsync()
        {
            return Provider.DisposeCacheAsync(this);
        }
    }
}
