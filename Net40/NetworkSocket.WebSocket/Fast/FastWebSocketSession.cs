﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// FastWebSocket的会话对象
    /// 不可继承
    /// </summary>
    public sealed class FastWebSocketSession : WebSocketSession, IFastWebSocketSession
    {
        /// <summary>
        /// 数据包id提供者
        /// </summary>
        private PacketIdProvider packetIdProvider;

        /// <summary>
        /// 任务行为表
        /// </summary>
        private TaskSetActionTable taskSetActionTable;

        /// <summary>
        /// 获取Json序列化工具       
        /// </summary>
        internal IJsonSerializer JsonSerializer { get; private set; }

        /// <summary>
        /// 获取全局滤过器
        /// </summary>
        internal GlobalFilters GlobalFilter { get; private set; }

        /// <summary>
        /// 获取Api行为特性过滤器提供者
        /// </summary>
        internal IFilterAttributeProvider FilterAttributeProvider { get; private set; }


        /// <summary>
        /// FastWebSocket的客户端对象
        /// </summary>
        /// <param name="packetIdProvider"></param>
        /// <param name="taskSetActionTable"></param>
        /// <param name="jsonSerializer"></param>
        /// <param name="filterAttributeProvider"></param>
        /// <param name="globalFilter"></param>
        internal FastWebSocketSession(PacketIdProvider packetIdProvider, TaskSetActionTable taskSetActionTable, IJsonSerializer jsonSerializer, IFilterAttributeProvider filterAttributeProvider, GlobalFilters globalFilter)
        {
            this.packetIdProvider = packetIdProvider;
            this.taskSetActionTable = taskSetActionTable;
            this.JsonSerializer = jsonSerializer;
            this.GlobalFilter = globalFilter;
            this.FilterAttributeProvider = filterAttributeProvider;
        }

        /// <summary>
        /// 调用远程端实现的服务方法        
        /// </summary>       
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param>  
        /// <exception cref="SocketException"></exception>      
        /// <exception cref="SerializerException"></exception>       
        public void InvokeApi(string api, params object[] parameters)
        {
            var packet = new FastPacket
            {
                api = api,
                id = this.packetIdProvider.NewId(),
                state = true,
                fromClient = false,
                body = parameters
            };
            var packetJson = this.JsonSerializer.Serialize(packet);
            this.SendText(packetJson);
        }

        /// <summary>
        /// 调用客户端实现的服务方法     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>        
        /// <param name="api">api</param>
        /// <param name="parameters">参数</param> 
        /// <exception cref="SocketException"></exception>         
        /// <exception cref="SerializerException"></exception>
        /// <returns>远程数据任务</returns>  
        public Task<T> InvokeApi<T>(string api, params object[] parameters)
        {
            var packet = new FastPacket
            {
                api = api,
                id = this.packetIdProvider.NewId(),
                state = true,
                fromClient = false,
                body = parameters
            };

            // 登记TaskSetAction       
            var taskSource = new TaskCompletionSource<T>();
            var taskSetAction = new TaskSetAction<T>(taskSource);
            taskSetActionTable.Add(packet.id, taskSetAction);

            var packetJson = this.JsonSerializer.Serialize(packet);
            this.SendText(packetJson);
            return taskSource.Task;
        }
    }
}
