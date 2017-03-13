//-----------------------------------------------------------------------
// <copyright file="CatConstants.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------
namespace AntServiceStack
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public class CatConstants
    {
        public const string SUCCESS = "0";
        public const string UNKNOWN_DOMAIN = "Unknown";
        public const string CONFIG_SERVICE_URL = "FxConfigServiceUrl";
        public const string CAT_SERVER = "CAT_SERVER";
        public const string LOCAL_CLIENT_CONFIG = "LocalClientConfig";
        public const string NULL_STRING = "null";
        public const string CAT_CONTEXT = "CatContext";
        public const string ID_MARK_FILE_MAP = "CatMarkFileMap";
        public const int ID_MARK_FILE_SIZE = 20;
        public const int ID_MARK_FILE_INDEX_OFFSET = 0;
        public const int ID_MARK_FILE_TS_OFFSET = 4;
        public const int ID_MARK_FILE_FLUSH_RATE = 1000;
        public const int HEARTBEAT_MIN_INITIAL_SLEEP_MILLISECONDS = 10000;
        public const int HEARTBEAT_MAX_INITIAL_SLEEP_MILLISECONDS = 60000;
        public const string DUMP_LOCKED = "dumpLocked";
        public const int TAGGED_TRANSACTION_CACHE_SIZE = 1024;
        public const int REFRESH_ROUTER_CONFIG_INTERVAL = 3600000;
        public const int TCP_RECONNECT_INTERVAL = 10000;
        public const int TCP_REBALANCE_INTERVAL = 600000;
        public const int TCP_CHECK_INTERVAL = 60000;
        public const int TCP_QUEUE_POLL_INTERVAL = 10;
        public const string ROOT_MESSAGE_ID = "RootMessageId";
        public const string CURRENT_MESSAGE_ID = "CurrentMessageId";
        public const string SERVER_MESSAGE_ID = "ServerMessageId";
        public const string CALL_APP = "CallApp";
        public const string TYPE_REMOTE_CALL = "RemoteCall";
        public const string NAME_REQUEST = "CallRequest";
        public const string TYPE_URL = "URL";
        public const string NAME_URL_CLIENT = "URL.client";
        public const string NAME_URL_METHOD = "URL.method";
        public const string TYPE_REMOTE_PORT = "Call.port";
        public const string TYPE_ESB_CALL = "ESBClient";
        public const string TYPE_ESB_CALL_SERVER = "ESBClient.serviceIP";
        public const string TYPE_ESB_CALL_APP = "ESBClient.serviceApp";
        public const string TYPE_ESB_SERVICE = "ESBService";
        public const string TYPE_ESB_SERVICE_CLIENT = "ESBService.clientIP";
        public const string TYPE_ESB_SERVICE_APP = "ESBService.clientApp";
        public const string TYPE_SOA_CALL = "SOA2Client";
        public const string TYPE_SOA_CALL_SERVER = "SOA2Client.serviceIP";
        public const string TYPE_SOA_CALL_APP = "SOA2Client.serviceApp";
        public const string TYPE_SOA_SERVICE = "SOA2Service";
        public const string TYPE_SOA_SERVICE_CLIENT = "SOA2Service.clientIP";
        public const string TYPE_SOA_SERVICE_APP = "SOA2Service.clientApp";
        public const string TYPE_SESSION_CALL = "SessionClient";
        public const string TYPE_SESSION_CALL_SERVER = "SessionClient.serviceIP";
        public const string TYPE_SESSION_CALL_APP = "SessionClient.serviceApp";
        public const string TYPE_SESSION_SERVICE = "SessionService";
        public const string TYPE_SESSION_SERVICE_CLIENT = "SessionService.clientIP";
        public const string TYPE_SESSION_SERVICE_APP = "SessionService.clientApp";
        public const string TYPE_CACHE_GET = "get";
        public const string TYPE_CACHE_MGET = "mget";
        public const string TYPE_CACHE_MISSED = "missed";
        public const string TYPE_CACHE_ADD = "add";
        public const string TYPE_CACHE_REMOVE = "remove";
        public const string TYPE_CACHE_MEMCACHED = "Memcached";
        public const string TYPE_CACHE_REDIS = "Redis";
        public const string TYPE_SQL = "SQL";
        public const string TYPE_SQL_METHOD = "SQL.method";
        public const string TYPE_SQL_DATABASE = "SQL.database";
        public const string LOG_ENABLE = "LogEnabled";
        public const string CLIENT_STATUS_SERVER_URL = "http://localhost:64582/cat-client/";
    }
}