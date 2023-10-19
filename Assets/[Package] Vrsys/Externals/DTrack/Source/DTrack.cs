﻿/* Copyright (c) 2019, Advanced Realtime Tracking GmbH
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of copyright holder nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DTrack.DataObjects;
using DTrack.Parser;
using UnityEngine;

namespace DTrack
{
    public class DTrack : MonoBehaviour
    {
        [Tooltip("Port for incoming DTrack tracking data")]
        public int listenPort = 5000;
        [Tooltip("Game objects receiving tracking data from DTrack")]
        public GameObject[] receivers = new GameObject[0];

        //public Axis axis = Axis.XZY;

        private IPEndPoint _endPoint;
        private UdpClient _client;
        private Thread _thread;

        private Packet _currentPacket;

        private bool _runReceiveThread = true;

        void Start()
        {
            //usedAxis = this.axis;

            _endPoint = new IPEndPoint(IPAddress.Any, listenPort);

            _client = new UdpClient(_endPoint);
            _thread = new Thread(async () =>
            {
                while (_runReceiveThread)
                {
                    try
                    {
                        var result = await _client.ReceiveAsync();
                        var rawString = Encoding.UTF8.GetString(result.Buffer);
                        //usedAxis = this.axis;
                        var packet = RawParser.Parse(rawString);
                        _currentPacket = packet;
                    }
                    catch (ObjectDisposedException) 
                    {
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Parsing Error: " + e);
                    }
                }
            });
            _thread.Start();

            //InvokeRepeating("FastUpdate", 0, (1f / 150f)); // 150Hz

            //Time.fixedDeltaTime = 1f / 60f; // set fixed update rate to 60Hz (global effect for all FixedUpdate functions)
        }

        public void RegisterTarget(GameObject obj)
        {
            var objs = new List<GameObject>(receivers);
            objs.Add(obj);
            receivers = objs.ToArray();
        }

        public void UnregisterTarget(GameObject obj)
        {
            var objs = new List<GameObject>(receivers);
            objs.Remove(obj);
            receivers = objs.ToArray();
        }

        void OnDestroy()
        {
            _runReceiveThread = false;
            _thread.Abort();
            _client.Close();
        }

        // Update is called once per frame
        void FixedUpdate() // here 60Hz
        {            
            if (_currentPacket != null)
            {
                foreach (var i in receivers)
                {
                    try
                    {
                        i.GetComponent<IDTrackReceiver>().ReceiveDTrackPacket(_currentPacket);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error passing Packet: " + e);
                    }
                }
            }
        }

        private void FastUpdate()
        {
            if (_currentPacket != null)
            {
                foreach (var i in receivers)
                {
                    try
                    {
                        i.GetComponent<IDTrackReceiver>().ReceiveDTrackPacket(_currentPacket);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error passing Packet: " + e);
                    }
                }
            }
        }
    }
}
