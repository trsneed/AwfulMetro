﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AwfulMetro.Common;
using AwfulMetro.Core.Entity;
using AwfulMetro.Core.Manager;
using AwfulMetro.Pcl.Core.Entity;
using AwfulMetro.Pcl.Core.Manager;
using AwfulMetro.Pcl.Core.Tools;

namespace AwfulMetro.ViewModels
{
    public class ThreadViewModel : NotifierBase
    {
        private ForumThreadEntity _forumThreadEntity;

        private string _html;

        private string _threadTitle;

        private bool _isLoading;

        private IEnumerable<int> _pageNumbers;

        public IEnumerable<int> PageNumbers
        {
            get { return _pageNumbers; }
            set
            {
                SetProperty(ref _pageNumbers, value);
                OnPropertyChanged();
            }
        }

        public ForumThreadEntity ForumThreadEntity
        {
            get { return _forumThreadEntity; }
            set
            {
                SetProperty(ref _forumThreadEntity, value);
                OnPropertyChanged();
            }
        }

        public string ThreadTitle
        {
            get { return _threadTitle; }
            set
            {
                SetProperty(ref _threadTitle, value);
                OnPropertyChanged();
            }
        }

        public string Html
        {
            get { return _html; }
            set
            {
                SetProperty(ref _html, value);
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged();
            }
        }

        public async void GetForumPosts()
        {
            IsLoading = true;
            ThreadTitle = ForumThreadEntity.Name;
            PostManager postManager = new PostManager();
            await postManager.GetThreadPosts(ForumThreadEntity);
            // Get the current reply count
            if (ForumThreadEntity.ScrollToPost != 0)
            {
                ForumThreadEntity.RepliesSinceLastOpened = ForumThreadEntity.RepliesSinceLastOpened - (40 - ForumThreadEntity.ScrollToPost);
            }
            else
            {
                ForumThreadEntity.RepliesSinceLastOpened = ForumThreadEntity.RepliesSinceLastOpened - ForumThreadEntity.ForumPosts.Count;
            }
            Html = await HtmlFormater.FormatThreadHtml(ForumThreadEntity);
            PageNumbers = Enumerable.Range(1, ForumThreadEntity.TotalPages).ToArray();
            OnPropertyChanged("ForumThreadEntity");
            IsLoading = false;
        }
    }
}
