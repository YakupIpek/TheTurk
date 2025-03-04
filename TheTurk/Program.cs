using System.Text;
using TheTurk.Engine;

Console.OutputEncoding = Encoding.UTF8;

var uciProtocol = new UCIProtocol();
await uciProtocol.Start();