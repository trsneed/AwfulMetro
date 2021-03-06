﻿#region copyright
/*
Awful Metro - A Modern UI Something Awful Forums Viewer
Copyright (C) 2014  Tim Miller

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software Foundation,
Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
*/
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using AwfulMetro.Core.Entity;
using AwfulMetro.Core.Tools;
using HtmlAgilityPack;

namespace AwfulMetro.Core.Manager
{
    public class ThreadManager
    {
        private readonly IWebManager _webManager;

        public ThreadManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public ThreadManager() : this(new WebManager())
        {
        }

        public async Task<HtmlDocument> GetThread(ForumThreadEntity forumThread, string url)
        {
            WebManager.Result result = await _webManager.GetData(url);
            HtmlDocument doc = result.Document;
            try
            {
                forumThread.ParseFromThread(doc);
            }
            catch (Exception)
            {
                return null;
            }
            string responseUri = result.AbsoluteUri;
            string[] test = responseUri.Split('#');
            if (test.Length > 1 && test[1].Contains("pti"))
            {
                forumThread.ScrollToPost = Int32.Parse(Regex.Match(responseUri.Split('#')[1], @"\d+").Value) - 1;
                forumThread.ScrollToPostString = string.Concat("#", responseUri.Split('#')[1]);
            }

            var query = Extensions.ParseQueryString(url);

            if (query.ContainsKey("pagenumber"))
            {
                forumThread.CurrentPage = Convert.ToInt32(query["pagenumber"]);
            }

            if (!query.ContainsKey("postid"))
            {
                return doc;
            }

            // If we are going to a post, it won't use #pti but instead uses the post id.

            forumThread.ScrollToPost = Convert.ToInt32(query["postid"]);
            forumThread.ScrollToPostString = "#postId" + query["postid"];

            return doc;

        }

        private string GetForumThreadCss(int forumId)
        {
            switch (forumId)
            {
                case 219:
                    return "219.css";
                case 26:
                    return "26.css";
                default:
                    return "default-forum.css";
            }
        }

        public async Task<bool> MarkPostAsLastRead(ForumThreadEntity threadEntity, int index)
        {
            await _webManager.GetData(string.Format(Constants.LAST_READ, index, threadEntity.ThreadId));
            return true;
        }

        private int ParseInt(string postClass)
        {
            string re1 = ".*?"; // Non-greedy match on filler
            string re2 = "(\\d+)"; // Integer Number 1

            var r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(postClass);
            if (!m.Success) return 0;
            String int1 = m.Groups[1].ToString();
            return Convert.ToInt32(int1);
        }

        public async Task<ObservableCollection<ForumThreadEntity>> GetBookmarks(ForumEntity forumCategory, int page)
        {
            var forumThreadList = new ObservableCollection<ForumThreadEntity>();
            String url = Constants.BOOKMARKS_URL;
            if (forumCategory.CurrentPage > 0)
            {
                url = Constants.BOOKMARKS_URL + string.Format(Constants.PAGE_NUMBER, page);
            }

            HtmlDocument doc = (await _webManager.GetData(url)).Document;

            HtmlNode forumNode =
                doc.DocumentNode.Descendants()
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("threadlist"));


            foreach (
                HtmlNode threadNode in
                    forumNode.Descendants("tr")
                        .Where(node => node.GetAttributeValue("class", string.Empty).StartsWith("thread")))
            {
                var threadEntity = new ForumThreadEntity();
                threadEntity.Parse(threadNode);
                threadEntity.IsBookmark = true;
                forumThreadList.Add(threadEntity);
            }
            return forumThreadList;
        }

        public async Task<bool> AddBookmarks(List<long> threadIdList)
        {
            foreach (long threadId in threadIdList)
            {
                await _webManager.PostData(
                    Constants.BOOKMARK, string.Format(
                        Constants.ADD_BOOKMARK, threadId
                        ));
            }
            return true;
        }

        public async Task<bool> RemoveBookmarks(List<long> threadIdList)
        {
            foreach (long threadId in threadIdList)
            {
                await _webManager.PostData(
                    Constants.BOOKMARK, string.Format(
                        Constants.REMOVE_BOOKMARK, threadId
                        ));
            }
            return true;
        }

        public async Task<bool> MarkThreadUnread(List<long> threadIdList)
        {
            foreach (long threadId in threadIdList)
            {
                await _webManager.PostData(
                    Constants.RESET_BASE, string.Format(
                        Constants.RESET_SEEN, threadId
                        ));
            }
            return true;
        }

        public async Task<ObservableCollection<ForumThreadEntity>> GetForumThreads(ForumEntity forumCategory, int page)
        {
            // TODO: Remove parsing logic from managers. I don't think they have a place here...
            string url = forumCategory.Location + string.Format(Constants.PAGE_NUMBER, page);

            HtmlDocument doc = (await _webManager.GetData(url)).Document;

            HtmlNode bodyNode = doc.DocumentNode.Descendants("body").First();

            int forumId = Convert.ToInt32(bodyNode.GetAttributeValue("data-forum", string.Empty));

            HtmlNode forumNode =
                doc.DocumentNode.Descendants()
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("threadlist"));
            var forumThreadList = new ObservableCollection<ForumThreadEntity>();
            foreach (
                HtmlNode threadNode in
                    forumNode.Descendants("tr")
                        .Where(node => node.GetAttributeValue("class", string.Empty).StartsWith("thread")))
            {
                var threadEntity = new ForumThreadEntity();
                threadEntity.Parse(threadNode);
                threadEntity.ForumId = forumId;
                forumThreadList.Add(threadEntity);
            }
            return forumThreadList;
        }

        public async Task<NewThreadEntity> GetThreadCookies(long forumId)
        {
            try
            {
                string url = string.Format(Constants.NEW_THREAD, forumId);
                WebManager.Result result = await _webManager.GetData(url);
                HtmlDocument doc = result.Document;

                HtmlNode[] formNodes = doc.DocumentNode.Descendants("input").ToArray();

                HtmlNode formKeyNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("formkey"));

                HtmlNode formCookieNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("form_cookie"));

                var newForumEntity = new NewThreadEntity();
                try
                {
                    string formKey = formKeyNode.GetAttributeValue("value", "");
                    string formCookie = formCookieNode.GetAttributeValue("value", "");
                    newForumEntity.FormKey = formKey;
                    newForumEntity.FormCookie = formCookie;
                    return newForumEntity;
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Could not parse new thread form data.");
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> CreateNewThread(NewThreadEntity newThreadEntity)
        {
            if (newThreadEntity == null)
                return false;
            var form = new MultipartFormDataContent
            {
                {new StringContent("postthread"), "action"},
                {new StringContent(newThreadEntity.Forum.ForumId.ToString(CultureInfo.InvariantCulture)), "forumid"},
                {new StringContent(newThreadEntity.FormKey), "formkey"},
                {new StringContent(newThreadEntity.FormCookie), "form_cookie"},
                {new StringContent(newThreadEntity.PostIcon.Id.ToString(CultureInfo.InvariantCulture)), "iconid"},
                {new StringContent(HtmlEncode(newThreadEntity.Subject)), "subject"},
                {new StringContent(HtmlEncode(newThreadEntity.Content)), "message"},
                {new StringContent(newThreadEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("Submit Reply"), "submit"}
            };
            HttpResponseMessage response = await _webManager.PostFormData(Constants.NEW_THREAD_BASE, form);

            return response.IsSuccessStatusCode;
        }

        public async Task<string> CreateNewThreadPreview(NewThreadEntity newThreadEntity)
        {
            if (newThreadEntity == null)
                return string.Empty;
            var form = new MultipartFormDataContent
            {
                {new StringContent("postthread"), "action"},
                {new StringContent(newThreadEntity.Forum.ForumId.ToString(CultureInfo.InvariantCulture)), "forumid"},
                {new StringContent(newThreadEntity.FormKey), "formkey"},
                {new StringContent(newThreadEntity.FormCookie), "form_cookie"},
                {new StringContent(newThreadEntity.PostIcon.Id.ToString(CultureInfo.InvariantCulture)), "iconid"},
                {new StringContent(HtmlEncode(newThreadEntity.Subject)), "subject"},
                {new StringContent(HtmlEncode(newThreadEntity.Content)), "message"},
                {new StringContent(newThreadEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("Submit Post"), "submit"},
                {new StringContent("Preview Post"), "preview"}
            };

            // We post to SA the same way we would for a normal reply, but instead of getting a redirect back to the
            // thread, we'll get redirected to back to the reply screen with the preview message on it.
            // From here we can parse that preview and return it to the user.

            HttpResponseMessage response = await _webManager.PostFormData(Constants.NEW_THREAD_BASE, form);
            Stream stream = await response.Content.ReadAsStreamAsync();
            using (var reader = new StreamReader(stream))
            {
                string html = reader.ReadToEnd();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode[] replyNodes = doc.DocumentNode.Descendants("div").ToArray();

                HtmlNode previewNode =
                    replyNodes.FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("inner postbody"));
                return previewNode == null ? string.Empty : FixPostHtml(previewNode.OuterHtml);
            }
        }

        private static string FixPostHtml(String postHtml)
        {
            return "<!DOCTYPE html><html>" + Constants.HTML_HEADER + "<body>" + postHtml + "</body></html>";
        }

        public static string HtmlEncode(string text)
        {
            // In order to get Unicode characters fully working, we need to first encode the entire post.
            // THEN we decode the bits we can safely pass in, like single/double quotes.
            // If we don't, the post format will be screwed up.
            char[] chars = WebUtility.HtmlEncode(text).ToCharArray();
            var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            result.Replace("&quot;", "\"");
            result.Replace("&#39;", @"'");
            result.Replace("&lt;", @"<");
            result.Replace("&gt;", @">");
            return result.ToString();
        }
    }
}