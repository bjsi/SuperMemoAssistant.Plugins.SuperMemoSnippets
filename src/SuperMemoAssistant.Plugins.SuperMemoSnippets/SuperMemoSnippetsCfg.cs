using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.SuperMemoSnippets
{
  [Form(Mode = DefaultFields.None)]
  [Title("Dictionary Settings",
       IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
      "Cancel",
      IsCancel = true)]
  [DialogAction("save",
      "Save",
      IsDefault = true,
      Validates = true)]
  public class SuperMemoSnippetsCfg : CfgBase<SuperMemoSnippetsCfg>, INotifyPropertyChangedEx
  {

    [Field(Name = "Name-Snippet Pairs (JSON)")]
    [MultiLine]
    public string Snippets { get; set; } = @"
{
  'def': 'The definition of _ is _.',
}";

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "SuperMemo Snippets";
    }

    public event PropertyChangedEventHandler PropertyChanged;

  }
}
