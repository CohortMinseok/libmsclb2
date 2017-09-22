# libmsclb2
A class library containing the essential functionality for a MapleStory Clientless Bot (CLB) in C#.
## Features
* Ready-to-use client, which handles incoming and outgoing data
* Stream-like readers and writers for packet interpretation and construction
* Distinction between internal and external packet headers (in other words: dynamic or not)
* Hardware Profile spoofing
* Nexon Passport web authentication (Credits to Waty)
## Getting Started
Building your own Clientless Bot on top of this library is actually **really** easy. In this description I'll give a brief explanation on the most essential things to get started.
### Basic Example
```csharp
//Oversimplified example of a Clientless Bot (CLB)
public class MyBot
{
    public MapleClient Client { get; set; }
    
    public CancellationTokenSource CTS { get; set; }
    
    private bool IsConnected { get; set; }
    
    public void StartBotting()
    {
        CTS = new CancellationTokenSource();
        Client = new MapleClient(); //This is the object that we will be using the most, as it combines the other essential features.
        Client.HandshakeReceived += OnHandshakeReceived;
        Client.PacketReceived += OnPacketReceived;
        
        //Create a thread or just call MainBotLoop
        MainBotLoop();
    }
    
    public void OnHandshakeReceived(ushort version, ushort subversion, byte locale, byte newbyte)
    {
        PacketWriter writer = new PacketWriter(0x0067); //Creates a new packet writer/builder with packet header 67 00. Encryption does not play a role here (yet).
        writer.WriteInt8(locale);
        writer.WriteUInt16(version);
        writer.WriteUInt16(subversion);
        writer.WriteInt8(newbyte);
        
        if(Client != null && Client.Connected) //Only send packets when the Client is initialized and connected to a server. This way we prevent causing unnecessary errors.
            Client.SendPacket(reader); //SendPacket takes care of encrypting the packet and passes it to the Socket
    }
    
    public void OnPacketReceived(PacketReader reader)
    {
        switch(reader.ExternalHeader) //ExternalHeader is the heawder sent by MapleStory. This library does not feature header decryption, but takes this into account by allowing you to set the InternalHeader yourself. This method would be the place to do so.
        {
            case 0x0000: //Some header that is not encrypted
                HandleSomePacket(reader);
                break;
            default: //All headers that are encrypted
                DecryptHeader(reader); //For example
                RouteDecryptedHeaderPacket(reader); //For example
                break;
        }
    }
    
    private void HandleSomePacket(PacketReader reader)
    {
        uint value1 = reader.ReadUInt32();
        string dynamicLength = reader.ReadMapleString();
        string staticlength = reader.ReadString(13);
        byte[] raw = reader.ReadBytes();
    }
    
    public void MainBotLoop() //We need to have a loop to keep receiving data and handling it, preferably on another thread than your UI
    {
        while(!CTS.IsCancellationRequested)
        {
            if(!IsConnected)
            {
                Client.Connect("127.0.0.1", 1337"); //IP/Port of MapleStory server
                IsConnected = true;
            }
            
            Client.Receive(); //Blocks until there is data. Data will be parsed and pushed to OnPacketReceived through an event
        }
    }
}
```
## Credits
* Waty for reversing the WebApi
* Yaminike/Minike for the FastAES implementation
* snow, haha01haha01 and jonyleeson for the original MapleLib(2)
## Disclaimer
I am not a professional programmer, in fact I never had a programming class in my life. The only reason this project exists is because of my interest in MapleStory hacking. I spent years messing around with CLB's and gained a lot from it. I felt like this is the time to give something back to the community.
