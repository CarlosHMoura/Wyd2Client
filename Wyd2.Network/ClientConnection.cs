﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WYD2.Common.GameStructure;
using WYD2.Common.IncomingPacketStructure;
using Outgoing = WYD2.Common.OutgoingPacketStructure;
using WYD2.Common.Utility;
using WYD2.Network;

namespace WYD2.Network
{
    public class ClientConnection : TcpBase
    {
        public event EventHandler<MLoginSuccessfulPacket> OnReceiveSucessfullLogin;
        public event EventHandler<ushort> OnReceiveUnknowPacket;
        public event EventHandler<bool> OnReceiveTokenResponse;
        public event EventHandler<EventArgs> OnReceiveCreateCharacterError;
        public event EventHandler<EventArgs> OnReceiveDeleteCharacterError;
        public event EventHandler<MResendCharListPacket> OnReceiveRefreshCharList;
        public event EventHandler<MCharToWorldPacket> OnReceiveCharToWorld;
        public event EventHandler<string> OnReceiveGameMessage;
        public event EventHandler<MCreateMobPacket> OnReceiveCreateMob;
        public event EventHandler<EventArgs> OnReceiveCharLogoutSignal;
        public event EventHandler<MChatMessagePacket> OnReceiveChatMessage;
        public event EventHandler<MSignalValuePacket> OnReceiveDeleteMob;
        public event EventHandler<MMovePacket> OnReceiveMovement;
        public new event EventHandler<EventArgs> OnSuccessfullConnect
        {
            add { base.OnSuccessfullConnect += value; }
            remove { base.OnSuccessfullConnect -= value; }
        }

        public ClientConnection(string ipAddress, int port) 
            : base(ipAddress, port)
        {
        }

        protected override void InterpretPacket(int packetId, byte[] buffer)
        {
            Console.WriteLine($"PacketId 0x{ packetId.ToString("X") }");

            var pHeader = W2Marshal.GetStructure<MPacketHeader>(buffer);

            PacketSecurity.LastPacket = Environment.TickCount;
            PacketSecurity.TimePacket = pHeader.TimeStamp;
            switch (packetId)
            {
                case MLoginSuccessfulPacket.Opcode:
                    OnReceiveSucessfullLogin?.Invoke(this, W2Marshal.GetStructure<MLoginSuccessfulPacket>(buffer));
                    break;
                case Outgoing.MTokenPacket.Opcode:
                case Outgoing.MTokenPacket.Opcode_Incorrect:
                    OnReceiveTokenResponse?.Invoke(this, packetId == Outgoing.MTokenPacket.Opcode);
                    break;
                case MResendCharListPacket.Opcode:
                case MResendCharListPacket.Opcode_DeleteCharcter:
                    OnReceiveRefreshCharList?.Invoke(this, W2Marshal.GetStructure<MResendCharListPacket>(buffer));
                    break;
                case 0x11A:
                    OnReceiveCreateCharacterError?.Invoke(this, EventArgs.Empty);
                    break;
                case 0x11B:
                    OnReceiveDeleteCharacterError?.Invoke(this, EventArgs.Empty);
                    break;
                case MCharToWorldPacket.Opcode:
                    OnReceiveCharToWorld?.Invoke(this, W2Marshal.GetStructure<MCharToWorldPacket>(buffer));
                    break;
                case MClientMessageTextPacket.Opcode:
                    OnReceiveGameMessage?.Invoke(this, W2Marshal.GetStructure<MClientMessageTextPacket>(buffer).Message);
                    break;
                case MCreateMobPacket.Opcode:
                    OnReceiveCreateMob?.Invoke(this, W2Marshal.GetStructure<MCreateMobPacket>(buffer));
                    break;
                case 0x116:
                    OnReceiveCharLogoutSignal?.Invoke(this, EventArgs.Empty);
                    break;
                case 0x165:
                    OnReceiveDeleteMob?.Invoke(this, W2Marshal.GetStructure<MSignalValuePacket>(buffer));
                    break;
                case MChatMessagePacket.Opcode:
                    OnReceiveChatMessage?.Invoke(this, W2Marshal.GetStructure<MChatMessagePacket>(buffer));
                    break;
                case MMovePacket.Opcode:
                    OnReceiveMovement?.Invoke(this, W2Marshal.GetStructure<MMovePacket>(buffer));
                    break;
                default:
                    OnReceiveUnknowPacket?.Invoke(this, (ushort)packetId);
                    break;
            }
        }
    }
}
