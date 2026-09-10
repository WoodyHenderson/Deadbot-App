using DeadBot.Services;

var root = Path.Combine(Path.GetTempPath(), "DeadBot-context-" + Guid.NewGuid());
void Put(string path, string content)
{
    var target = Path.Combine(root, path);
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    File.WriteAllText(target, content);
}
void Check(bool value, string name)
{
    if (!value) throw new Exception(name);
    Console.WriteLine("PASS: " + name);
}
try
{
    Put("general/game-loop.md", "Core loop");
    Put("data/economy.yaml", "souls: 1");
    Put("heroes/haze/haze.md", "Haze abilities");
    Put("heroes/haze/haze.yaml", "name: Haze");
    Put("heroes/mo-and-krill/mo-and-krill.md", "Mo and Krill");
    Put("items/spirit-lifesteal/spirit-lifesteal.yaml", "name: Spirit Lifesteal");
    Put("patches/old.md", "outdated facts");
    var builder = new KnowledgebaseContextBuilder();
    var core = await builder.BuildAsync(root, "How does the game work?", []);
    Check(core.Sources.Count == 2, "core always included, patches and unrelated entities excluded");
    var named = await builder.BuildAsync(root, "HAZE and Spirit Lifesteal?", []);
    Check(named.Sources.Count == 5, "complete named hero and item files included");
    var followup = await builder.BuildAsync(root, "What about her abilities?", ["Haze?"]);
    Check(followup.Sources.Contains("heroes/haze/haze.yaml"), "follow-up retains explicit entities");
    var boundary = await builder.BuildAsync(root, "hazel", []);
    Check(boundary.Sources.Count == 2, "no substring match");
    var punctuation = await builder.BuildAsync(root, "Mo & Krill?", []);
    Check(punctuation.Sources.Contains("heroes/mo-and-krill/mo-and-krill.md"), "punctuation normalization");
    Check(named.Text.Contains("SOURCE: heroes/haze/haze.yaml"), "stable path citation labels");
    Put("general/large.md", new string('a', KnowledgebaseContextBuilder.MaximumCharacters));
    try { await builder.BuildAsync(root, "Haze", []); throw new Exception("limit not enforced"); }
    catch (InvalidOperationException) { Console.WriteLine("PASS: oversized evidence rejected"); }
    try { await builder.BuildAsync(root + "-missing", "Hi", []); throw new Exception("missing not detected"); }
    catch (InvalidOperationException) { Console.WriteLine("PASS: missing repository rejected"); }
}
finally { Directory.Delete(root, true); }
