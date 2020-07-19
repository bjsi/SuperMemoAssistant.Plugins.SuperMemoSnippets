using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.SuperMemoSnippets
{

  public interface IControlHtmlEvent
  {
    event EventHandler<IControlHtmlEventArgs> OnEvent;
    public void handler(IHTMLEventObj e);
  }

  [ComVisible(true)]
  [ClassInterface(ClassInterfaceType.AutoDispatch)]
  public class OnKeyUpEvent : IControlHtmlEvent
  {

    public event EventHandler<IControlHtmlEventArgs> OnEvent;

    public IHTMLElement spanElement { get; set; }
    public IHTMLElement bodyElement { get; set; }

    public OnKeyUpEvent(IHTMLElement span, IHTMLElement body)
    {
      this.spanElement = span;
      this.bodyElement = body;
    }

    [DispId(0)]
    public void handler(IHTMLEventObj e)
    {
      if (!OnEvent.IsNull())
        OnEvent(this, new IControlHtmlEventArgs(e, spanElement, bodyElement));
    }
  }

  [ComVisible(true)]
  [ClassInterface(ClassInterfaceType.AutoDispatch)]
  public class OnKeyDownEvent : IControlHtmlEvent
  {
    public event EventHandler<IControlHtmlEventArgs> OnEvent;
    public IHTMLElement spanElement { get; set; }
    public IHTMLElement bodyElement { get; set; }
    public OnKeyDownEvent(IHTMLElement span, IHTMLElement body)
    {
      this.spanElement = span;
      this.bodyElement = body;
    }

    [DispId(0)]
    public void handler(IHTMLEventObj e)
    {
      if (!OnEvent.IsNull())
        OnEvent(this, new IControlHtmlEventArgs(e, spanElement, bodyElement));
    }
  }

  public class IControlHtmlEventArgs
  {
    public IHTMLEventObj EventObj { get; set; }
    public IHTMLElement spanElement { get; set; }
    public IHTMLElement bodyElement { get; set; }
    public IControlHtmlEventArgs(IHTMLEventObj EventObj, IHTMLElement span, IHTMLElement body)
    {
      this.EventObj = EventObj;
      this.spanElement = span;
      this.bodyElement = body;
    }
  }

}
