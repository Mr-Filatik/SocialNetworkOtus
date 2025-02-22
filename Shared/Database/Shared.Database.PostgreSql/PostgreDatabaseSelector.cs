﻿using Microsoft.Extensions.Logging;
using Npgsql;
using SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;

namespace SocialNetworkOtus.Shared.Database.PostgreSql;

public class PostgreDatabaseSelector
{
    private NpgsqlDataSource _dataMasterSource;
    private List<NpgsqlDataSource> _dataReplicaSources;
    private List<NpgsqlDataSource> _dataShardSources;
    private int _replicaIndex = 0;
    private int _shardIndex = 0;

    private readonly PostgresOptions _options;
    private readonly ILogger<PostgreDatabaseSelector> _logger;

    public PostgreDatabaseSelector(PostgresOptions options, ILogger<PostgreDatabaseSelector> logger)
    {
        _options = options;
        _logger = logger;
    }

    public NpgsqlDataSource GetDatabase(bool onlyRead = false, bool withSharding = false)
    {
        //if (withSharding)
        //{
        //    if (_dataShardSources == null || _dataShardSources.Count == 0)
        //    {
        //        _dataShardSources = new List<NpgsqlDataSource>();
        //        foreach (var conn in _options.ShardConnectionStrings)
        //        {
        //            _dataShardSources.Add(NpgsqlDataSource.Create(conn));
        //        }
        //    }

        //    var currentSource = _dataShardSources[_shardIndex];
        //    _shardIndex = (_shardIndex + 1) % _dataShardSources.Count;
        //    return currentSource;
        //}

        //if (onlyRead)
        //{
        //    if (_dataReplicaSources == null || _dataReplicaSources.Count == 0)
        //    {
        //        _dataReplicaSources = new List<NpgsqlDataSource>();
        //        foreach (var conn in _options.ReplicaConnectionStrings)
        //        {
        //            _dataReplicaSources.Add(NpgsqlDataSource.Create(conn));
        //        }
        //    }

        //    var currentSource = _dataReplicaSources[_replicaIndex];
        //    //_logger.LogInformation($"Get replica connection {currentSource.ConnectionString}");
        //    _replicaIndex = (_replicaIndex + 1) % _dataReplicaSources.Count;
        //    return currentSource;
        //}

        if (_dataMasterSource is null)
        {
            _dataMasterSource = NpgsqlDataSource.Create(_options.MasterConnectionString);
        }
        //_logger.LogInformation($"Get master connection {_options.MasterConnectionString}");
        return _dataMasterSource;
    }
}
