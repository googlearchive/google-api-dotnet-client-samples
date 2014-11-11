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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

using GalaSoft.MvvmLight;

using Blogger.Sample.Common;
using Blogger.Sample.Repository;

namespace Blogger.Sample.ViewModel
{
    /// <summary>Single blog view model. This view model holds the data about the blog's posts.</summary>
    public class BlogViewModel : ViewModelBase
    {
        /// <summary>Gets the post view models. Updating this collection should be from the UI thread.</summary>
        public ObservableCollection<PostViewModel> Posts { get; private set; }

        private string id;
        /// <summary>Gets or sets the blog id.</summary>
        public string Id 
        {
            get { return id; }
            set
            {
                Set(() => Id, ref id, value);
            }
        }

        private string name;
        /// <summary>Gets or sets the blog name.</summary>
        public string Name 
        { 
            get { return name; }
            set
            {
                Set(() => Name, ref name, value);
            }
        }

        private readonly IBloggerRepository repository;

        public BlogViewModel(IBloggerRepository repository)
        {
            this.repository = repository;
            Posts = new ObservableCollection<PostViewModel>();
        }

        internal void RefreshPosts()
        {
            Task.Run(async () => await RefreshPostsAsync());
        }

        /// <summary>
        /// Asynchronously refreshes the posts. It calls the repository get posts method in order to refill the
        /// <see cref="Posts"/> collection.
        /// </summary>
        internal async Task RefreshPostsAsync()
        {
            var posts = await repository.GetPostsAsync(id);
            await UIUtils.InvokeFromUIThread(() =>
            {
                Posts.Clear();
                foreach (var p in posts)
                {
                    Posts.Add(new PostViewModel
                        {
                            Title = p.Title,
                            Content = p.Content
                        });
                }
            });
        }
    }
}
