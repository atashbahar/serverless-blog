using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using ServerlessBlog.Application;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerlessBlog.Processor
{
    public static class MarkdownProcessor
    {
        public static async Task<Post> GetPost(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .Build();

            var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);

            var document = Markdown.Parse(markdown, pipeline);
            var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (yamlBlock == null)
            {
                throw new Exception("FrontMatter cannot be null");
            }

            var yaml = yamlBlock.Lines.ToString();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var post = deserializer.Deserialize<Post>(yaml);

            renderer.Render(document);
            await writer.FlushAsync();
            post.Body = writer.ToString();

            return post;
        }
    }
}
