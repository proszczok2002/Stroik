using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using System.Text;
string Template(ExpressionSyntax exp) {
 if(exp is LiteralExpressionSyntax text)return text.Token.ValueText;
 if(exp is InterpolatedStringExpressionSyntax format) {
   var b=new StringBuilder();int i=0;
   foreach(var part in format.Contents)if(part is InterpolatedStringTextSyntax t)b.Append(t.TextToken.ValueText);else if(part is InterpolationSyntax value)b.Append("{"+(i++)+(value.FormatClause is null?"":":"+value.FormatClause.FormatStringToken.ValueText)+"}");
   return b.ToString();
 }
 return null;
}
var pairs=new SortedDictionary<string,string>();
foreach(var file in Directory.GetFiles(".","*.cs")) {
 var root=CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
 foreach(var call in root.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
  int offset=call.Expression.ToString() is "UiLanguages.Get" or "UiLanguages.Format" ? 1 : 0;
  if(offset==0 && call.Expression.ToString() is not ("T" or "Tf" or "SetUpdateStatus" or "SetUpdateStatusF"))continue;
  if(call.ArgumentList.Arguments.Count!=2+offset)continue;
  string pl=Template(call.ArgumentList.Arguments[offset].Expression),en=Template(call.ArgumentList.Arguments[offset+1].Expression);
  if(en!=null && pl!=null)pairs[en]=pl;
 }
}
Directory.CreateDirectory("locales");File.WriteAllText("locales/source-csharp.json",JsonSerializer.Serialize(pairs,new JsonSerializerOptions{WriteIndented=true,Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}));
Console.WriteLine($"{pairs.Count} translation templates extracted");
