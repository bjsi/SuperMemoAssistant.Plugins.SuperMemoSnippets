#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   7/9/2020 6:12:21 AM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.SuperMemoSnippets
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;
  using System.Runtime.Remoting;
  using System.Windows.Input;
  using Anotar.Serilog;
  using Gma.DataStructures.StringSearch;
  using mshtml;
  using SuperMemoAssistant.Extensions;
  using SuperMemoAssistant.Plugins.Autocompleter.Interop;
  using SuperMemoAssistant.Services;
  using SuperMemoAssistant.Services.IO.Keyboard;
  using SuperMemoAssistant.Services.Sentry;
  using SuperMemoAssistant.Sys.IO.Devices;
  using SuperMemoAssistant.Sys.Remoting;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class SuperMemoSnippetsPlugin : SentrySMAPluginBase<SuperMemoSnippetsPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public SuperMemoSnippetsPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion


    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "SuperMemoSnippets";

    /// <inheritdoc />
    public override bool HasSettings => false;
    public SuperMemoSnippetsCfg Config { get; set; }
    private IAutocompleterSvc _autocompleterSvc { get; set; }

    // Events
    private OnKeyUpEvent _keyup { get; set; }
    private OnKeyDownEvent _keydown { get; set; }

    // Snippets
    private Dictionary<string, string> SnippetMap => CreateSnippetMap();
    private Trie<string> SnippetTrie { get; set; }

    #endregion

    #region Methods Impl

    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<SuperMemoSnippetsCfg>() ?? new SuperMemoSnippetsCfg();
    }

    /// <inheritdoc />
    protected override void PluginInit()
    {

      LoadConfig();

      Svc.HotKeyManager.RegisterGlobal(
        "SearchSnippets",
        "Search your snippets",
        HotKeyScopes.SMBrowser,
        new HotKey(Key.S, KeyModifiers.CtrlAltShift),
        SearchSnippets
      );

      _autocompleterSvc = GetService<IAutocompleterSvc>();

      SnippetMap?.Keys?.ForEach(k => SnippetTrie.Add(k, k));

    }

    [LogToErrorOnException]
    private void SearchSnippets()
    {
      try
      {

        if (_autocompleterSvc.IsNull())
        {
          LogTo.Error("Failed to SearchSnippets because _autocompleterSvc is null");
          return;
        }

        var selObj = ContentUtils.GetSelectionObject();
        if (selObj.IsNull() || !selObj.text.IsNullOrEmpty()) // cancel if there is selected text
          return;

        var htmlDoc = ContentUtils.GetFocusedHTMLDocument();
        if (htmlDoc.IsNull())
          return;

        string id = Guid.NewGuid().ToString();
        selObj.pasteHTML($"<span id='{id}' style='color: blue;'>" + "&nbsp;_&nbsp;" + "</span>");

        var span = htmlDoc.all
          ?.Cast<IHTMLElement>()
          ?.Where(x => x.id == id)
          ?.FirstOrDefault();

        if (span.IsNull())
          return;

        selObj.moveToElementText(span);
        selObj.moveStart("character", 1);
        selObj.moveEnd("character", -1);
        selObj.select();

        var body = htmlDoc.body;

        if (!_autocompleterSvc.SetWordSuggestionSource(Name, SnippetTrie, SnippetMap))
        {
          LogTo.Error("Failed to SetWordSuggestionSource");
          return;
        }
        _autocompleterSvc.OnSuggestionAccepted += new ActionProxy<SuggestionAcceptedEventArgs>((e) => OnSuggestionAccepted(span, body, e));

      }
      catch (RemotingException) { }

    }

    [LogToErrorOnException]
    private void OnSuggestionAccepted(IHTMLElement span, IHTMLElement body, SuggestionAcceptedEventArgs obj)
    {
      try
      {

        if (obj.IsNull())
          return;

        if (obj.PluginName != Name)
          return;

        if (_autocompleterSvc.IsNull())
        {
          LogTo.Error("Failed to reset autocompleterSvc suggestion source because the svc object is null");
          return;
        }

        _autocompleterSvc.ResetWordSuggestionSource();

        //// Add snippet activation event
        _keydown = new OnKeyDownEvent(span, body);
        ((IHTMLElement2)body).SubscribeTo(EventType.onkeydown, _keydown);
        _keydown.OnEvent += BodyOnKeyDownNavigateSnippet;

        // Add remove navigation events event
        _keyup = new OnKeyUpEvent(span, body);
        ((IHTMLElement2)body).SubscribeTo(EventType.onkeyup, _keyup);
        _keyup.OnEvent += OnKeyUp_RemoveNavigationEvent;


      }
      catch (RemotingException) { }

    }

    private void RemoveKeyUpEvent(IHTMLElement body)
    {
      ((IHTMLElement2)body)?.UnsubscribeFrom(EventType.onkeyup, _keyup);
    }

    [LogToErrorOnException]
    private void BodyOnKeyDownNavigateSnippet(object sender, IControlHtmlEventArgs e)
    {
      try
      {

        var eventObj = e?.EventObj;
        if (eventObj.IsNull())
          return;

        var span = e.spanElement;

        // Check keys
        bool ctrlPressed = eventObj.shiftKey;
        bool jPressed = eventObj.keyCode == 74;
        bool kPressed = eventObj.keyCode == 75;

        if (ctrlPressed && (jPressed || kPressed))
        {

          var htmlDoc = ContentUtils.GetFocusedHTMLDocument();
          if (htmlDoc.IsNull())
            return;

          var selObj = htmlDoc.selection.createRange() as IHTMLTxtRange;
          var duplicate = selObj.duplicate();

          // Check the event origin
          var src = selObj.parentElement();
          var children = span.children as IHTMLElementCollection;

          // If either the event source == target 
          // or some child of the event source == target...
          if (span.id == src.id
            || (!children.IsNull() && children.Cast<IHTMLElement>().Any(x => x == src)))
          {

            if (jPressed)
            {

              duplicate.moveToElementText(span);
              duplicate.setEndPoint("EndToEnd", selObj);
              if (duplicate.findText("_", -2000000000))
                duplicate.select();

            }
            else if (kPressed)
            {

              duplicate.moveToElementText(span);
              duplicate.setEndPoint("StartToStart", selObj);
              if (duplicate.findText("_"))
                duplicate.select();

            }

          }
        }
      }
      catch (RemotingException) { }
    }

    // TODO: pass span and body into the method instead
    private void OnKeyUp_RemoveNavigationEvent(object sender, IControlHtmlEventArgs e)
    {

      var span = e.spanElement;
      var body = e.bodyElement;

      // If there are no underscores left in the span, remove all of the events
      if (!span.IsNull() && !span.innerText.Contains("_"))
      {
        RemoveKeyDownEvent(body);
        RemoveKeyUpEvent(body);
      }

    }

    private void RemoveKeyDownEvent(IHTMLElement body)
    {
      ((IHTMLElement2)body)?.UnsubscribeFrom(EventType.onkeydown, _keydown);
    }

    private Dictionary<string, string> CreateSnippetMap()
    {
      return Config?.Snippets?.Deserialize<Dictionary<string, string>>();
    }

    /// <inheritdoc />
    public override void ShowSettings()
    {
    }

    #endregion

    #region Methods

    #endregion
  }
}
