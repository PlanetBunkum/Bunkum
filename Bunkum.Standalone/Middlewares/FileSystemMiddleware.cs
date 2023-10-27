using System.Net;
using System.Text.Encodings.Web;
using System.Web;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;
using Bunkum.Protocols.Gopher;
using Bunkum.Protocols.Http;

namespace Bunkum.Standalone.Middlewares;

public class FileSystemMiddleware : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        string path = Path.Join(Environment.CurrentDirectory, HttpUtility.UrlDecode(context.Uri.AbsolutePath));
        if (!Path.Exists(path)) return;

        if (Directory.Exists(path))
        {
            this.HandleResponseSafe(context, path, this.HandleDirectory);
        }
        else if (File.Exists(path))
        {
            this.HandleResponseSafe(context, path, this.HandleFile);
        }
        else
        {
            context.ResponseCode = HttpStatusCode.NotFound;
            return;
        }
    }

    private void HandleResponseSafe(ListenerContext context, string path, Action<ListenerContext, string> handler)
    {
        try
        {
            handler.Invoke(context, path);
        }
        catch (UnauthorizedAccessException)
        {
            context.ResponseCode = HttpStatusCode.Forbidden;
        }
        catch
        {
            context.ResponseCode = HttpStatusCode.InternalServerError;
        }
    }
    
    private void HandleFile(ListenerContext context, string path)
    {
        byte[] data = File.ReadAllBytes(path);
            
        context.ResponseCode = HttpStatusCode.OK;
        context.Write(data);

        if (context.Protocol.Name == GopherProtocolInformation.Gopher.Name)
        {
            context.Write("\r\n.\r\n");
        }
    }
    
    private void HandleDirectory(ListenerContext context, string path)
    {
        context.ResponseCode = HttpStatusCode.OK;
        
        string name = context.Protocol.Name;
        bool isHttp = name == HttpProtocolInformation.Http1_1.Name;
        bool isGopher = name == GopherProtocolInformation.Gopher.Name;

        if (isHttp)
        {
            context.ResponseType = "text/html";
            context.Write("<!DOCTYPE html>\r\n<html><body>");
        }

        foreach (string fullDirectory in Directory.GetDirectories(path))
        {
            string directory = fullDirectory.Replace(Environment.CurrentDirectory, "");
            if (isHttp)
            {
                context.Write($"<a href=\"{directory}\">{Path.GetFileName(directory)}/</a><br>\r\n");
            }
            else if (isGopher)
            {
                context.Write($"1{Path.GetFileName(directory)}\t{directory}\tlocalhost\t10061\r\n");
            }
        }
        
        foreach (string fullFile in Directory.GetFiles(path))
        {
            string file = fullFile.Replace(Environment.CurrentDirectory, "");
            if (isHttp)
            {
                context.Write($"<a href=\"{file}\">{Path.GetFileName(file)}</a><br>\r\n");
            }
            else if (isGopher)
            {
                char type = '0';
                string extension = Path.GetExtension(file);

                if (extension is ".png" or ".jpg") type = 'I'; 
                
                context.Write($"{type}{Path.GetFileName(file)}\t{file}\tlocalhost\t10061\r\n");
            }
        }

        if (isHttp) context.Write("</body></html>");
        else if(isGopher) context.Write(".\r\n");
    }
}