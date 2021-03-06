﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// Fast会话对象
    /// 不可继承
    /// </summary>
    public sealed class FastSession : SessionBase, IFastSession
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
        /// 获取序列化工具      
        /// </summary>
        internal ISerializer Serializer { get; private set; }

        /// <summary>
        /// 获取全局滤过器
        /// </summary>
        internal GlobalFilters GlobalFilter { get; private set; }

        /// <summary>
        /// Api行为特性过滤器提供者
        /// </summary>
        internal IFilterAttributeProvider FilterAttributeProvider { get; set; }

        /// <summary>
        /// 服务器的客户端对象
        /// </summary>
        /// <param name="serializer">序列化工具</param>
        /// <param name="packetIdProvider">数据包id提供者</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="filterAttributeProvider">特性过滤器提供者</param>
        /// <param name="globalFilter">全局过滤器</param>
        internal FastSession(PacketIdProvider packetIdProvider, TaskSetActionTable taskSetActionTable, ISerializer serializer, IFilterAttributeProvider filterAttributeProvider, GlobalFilters globalFilter)
        {
            this.packetIdProvider = packetIdProvider;
            this.taskSetActionTable = taskSetActionTable;
            this.Serializer = serializer;
            this.GlobalFilter = globalFilter;
            this.FilterAttributeProvider = filterAttributeProvider;
        }

        /// <summary>
        /// 调用远程端实现的Api        
        /// </summary>        
        /// <param name="api">数据包Api名</param>
        /// <param name="parameters">参数列表</param>      
        /// <exception cref="SocketException"></exception>     
        /// <exception cref="SerializerException"></exception>   
        /// <exception cref="ProtocolException"></exception>
        public void InvokeApi(string api, params object[] parameters)
        {
            var id = this.packetIdProvider.NewId();
            var packet = new FastPacket(api, id, false);
            packet.SetBodyParameters(this.Serializer, parameters);
            this.Send(packet.ToByteRange());
        }

        /// <summary>
        /// 调用远程端实现的Api      
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>        
        /// <param name="api">数据包Api名</param>
        /// <param name="parameters">参数</param>       
        /// <exception cref="SocketException"></exception>      
        /// <exception cref="SerializerException"></exception>
        /// <exception cref="ProtocolException"></exception>
        /// <returns>远程数据任务</returns>         
        public Task<T> InvokeApi<T>(string api, params object[] parameters)
        {
            var id = this.packetIdProvider.NewId();
            var packet = new FastPacket(api, id, false);
            packet.SetBodyParameters(this.Serializer, parameters);
            return FastTcpCommon.InvokeApi<T>(this, this.taskSetActionTable, this.Serializer, packet);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.taskSetActionTable.Dispose();
        }
    }
}
