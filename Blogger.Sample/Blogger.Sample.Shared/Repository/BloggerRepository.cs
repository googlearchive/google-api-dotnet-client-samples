/*
Copyright 2014 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Blogger.v3;
using Google.Apis.Services;

namespace Blogger.Sample.Repository
{
    /// <summary>The blogger repository implementation which works the same for Windows and Windows Phone.</summary>
    public class BloggerRepository : IBloggerRepository
    {
        private UserCredential credential;
        private BloggerService service;

        private async Task AuthenticateAsync()
        {
            if (service != null)
                return;

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new Uri("ms-appx:///Assets/client_secrets.json"),
                new[] { BloggerService.Scope.BloggerReadonly },
                "user",
                CancellationToken.None);

            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BloggerApp",
            };

            service = new BloggerService(initializer);
        }

        #region IBloggerRepository members

        public async Task<IEnumerable<Blog>> GetBlogsAsync()
        {
            await AuthenticateAsync();

            var list = await service.Blogs.ListByUser("self").ExecuteAsync();
            return from blog in list.Items
                   select new Blog
                   {
                       Id = blog.Id,
                       Name = blog.Name
                   };
        }

        public async Task<IEnumerable<Post>> GetPostsAsync(string blogId)
        {
            await AuthenticateAsync();
            var list = await service.Posts.List(blogId).ExecuteAsync();
            return from post in list.Items
                   select new Post
                   {
                        Title = post.Title,
                        Content = post.Content
                   };
        }

        #endregion
    }
}
