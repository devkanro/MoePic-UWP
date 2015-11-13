using System.Net;

namespace MoePic.Toolkit.Net
{
    /// <summary>
    /// 表示对一个<see cref="HttpWebRequest"/>进行操作的委托。
    /// </summary>
    /// <param name="request">需要进行操作的<see cref="HttpWebRequest"/></param>
    public delegate void HttpWebRequestHandler(HttpWebRequest request);
}
