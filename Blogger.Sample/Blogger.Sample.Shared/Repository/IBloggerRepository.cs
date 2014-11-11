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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blogger.Sample.Repository
{
    /// <summary>Blog data contact.</summary>
    public class Blog
    {
        /// <summary>Gets or sets the blog id.</summary>
        public string Id { get; set; }
        /// <summary>Gets or sets the blog name.</summary>
        public string Name { get; set; }
    }

    /// <summary>Post data contact.</summary>
    public class Post
    {
        /// <summary>Gets or sets the post title.</summary>
        public string Title { get; set; }
        /// <summary>Gets or sets the post content.</summary>
        public string Content { get; set; }
    }

    /// <summary>Blogger repository for retrieving blogs and posts.</summary>
    public interface IBloggerRepository
    {
        /// <summary>Gets all post for the specified blog.</summary>
        Task<IEnumerable<Post>> GetPostsAsync(string blogId);

        /// <summary>Get all user's blogs.</summary>
        Task<IEnumerable<Blog>> GetBlogsAsync();
    }
}
