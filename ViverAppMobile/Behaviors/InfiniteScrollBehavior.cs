using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;

namespace ViverAppMobile.Behaviors
{
    public partial class InfiniteScrollBehavior : Behavior<CollectionView>
    {
        public static readonly BindableProperty LoadMoreCommandProperty =
            BindableProperty.Create(nameof(LoadMoreCommand), typeof(ICommand), typeof(InfiniteScrollBehavior));

        public static readonly BindableProperty FooterViewProperty =
            BindableProperty.Create(nameof(FooterView), typeof(View), typeof(InfiniteScrollBehavior));

        public static readonly BindableProperty EmptyViewProperty =
            BindableProperty.Create(nameof(EmptyView), typeof(View), typeof(InfiniteScrollBehavior));

        public static readonly BindableProperty HasMoreItemsProperty =
            BindableProperty.Create(
                nameof(HasMoreItems),
                typeof(bool),
                typeof(InfiniteScrollBehavior),
                true,
                propertyChanged: OnHasMoreItemsChanged);

        public static readonly BindableProperty RemainingItemsThresholdProperty =
            BindableProperty.Create(
                nameof(RemainingItemsThreshold),
                typeof(int),
                typeof(InfiniteScrollBehavior),
                2);

        public static readonly BindableProperty TargetScrollViewProperty =
            BindableProperty.Create(
                nameof(TargetScrollView),
                typeof(ScrollView),
                typeof(InfiniteScrollBehavior),
                propertyChanged: OnTargetScrollViewChanged);

        private CollectionView? _collectionView;
        private ScrollView? _scrollView;
        private bool _isLoading;
        private INotifyCollectionChanged? _notifyCollectionChanged;


        public ICommand? LoadMoreCommand
        {
            get => (ICommand?)GetValue(LoadMoreCommandProperty);
            set => SetValue(LoadMoreCommandProperty, value);
        }

        public View? FooterView
        {
            get => (View?)GetValue(FooterViewProperty);
            set => SetValue(FooterViewProperty, value);
        }

        public View? EmptyView
        {
            get => (View?)GetValue(EmptyViewProperty);
            set => SetValue(EmptyViewProperty, value);
        }

        public bool HasMoreItems
        {
            get => (bool)GetValue(HasMoreItemsProperty);
            set => SetValue(HasMoreItemsProperty, value);
        }

        public int RemainingItemsThreshold
        {
            get => (int)GetValue(RemainingItemsThresholdProperty);
            set => SetValue(RemainingItemsThresholdProperty, value);
        }

        public ScrollView? TargetScrollView
        {
            get => (ScrollView?)GetValue(TargetScrollViewProperty);
            set => SetValue(TargetScrollViewProperty, value);
        }

        protected override void OnAttachedTo(CollectionView bindable)
        {
            base.OnAttachedTo(bindable);
            _collectionView = bindable;

            this.BindingContext = bindable.BindingContext;
            bindable.BindingContextChanged += (s, e) =>
            {
                this.BindingContext = bindable.BindingContext;
            };

            bindable.PropertyChanged += OnCollectionViewPropertyChanged;

            MainThread.BeginInvokeOnMainThread(AttachScrollHandler);

            AttachItemsSource(bindable.ItemsSource);
            UpdateViewsVisibility();
        }

        protected override void OnDetachingFrom(CollectionView bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.PropertyChanged -= OnCollectionViewPropertyChanged;
            DetachItemsSource();
            DetachScrollHandler();

            _collectionView = null;
            _scrollView = null;
        }

        private static void OnTargetScrollViewChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var behavior = (InfiniteScrollBehavior)bindable;
            behavior.DetachScrollHandler();
            behavior.AttachScrollHandler();
        }

        private void AttachScrollHandler()
        {
            if (_collectionView == null)
                return;

            _scrollView = TargetScrollView ?? FindParentScrollView(_collectionView);

            if (_scrollView != null)
                _scrollView.Scrolled += OnScrollViewScrolled;
            else
                _collectionView.Scrolled += OnCollectionViewScrolled;
        }

        private void DetachScrollHandler()
        {
            if (_scrollView != null)
                _scrollView.Scrolled -= OnScrollViewScrolled;
            if (_collectionView != null)
                _collectionView.Scrolled -= OnCollectionViewScrolled;
        }

        private void OnCollectionViewPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CollectionView.ItemsSource))
            {
                DetachItemsSource();
                AttachItemsSource(_collectionView?.ItemsSource);
                UpdateViewsVisibility();
            }
        }

        private void AttachItemsSource(object? itemsSource)
        {
            if (itemsSource is INotifyCollectionChanged ncc)
            {
                _notifyCollectionChanged = ncc;
                ncc.CollectionChanged += OnCollectionChanged;
            }
        }

        private void DetachItemsSource()
        {
            if (_notifyCollectionChanged != null)
            {
                _notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
                _notifyCollectionChanged = null;
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _isLoading = false;
            UpdateViewsVisibility();
        }

        private static void OnHasMoreItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var behavior = (InfiniteScrollBehavior)bindable;
            behavior.UpdateViewsVisibility();
        }

        private int GetItemsCount()
        {
            if (_collectionView?.ItemsSource == null)
                return 0;

            if (_collectionView.ItemsSource is ICollection col)
                return col.Count;

            return _collectionView.ItemsSource.Cast<object>().Count();
        }

        private void UpdateViewsVisibility()
        {
            if (_collectionView == null)
                return;

            int count = GetItemsCount();

            if (FooterView != null)
                FooterView.IsVisible = HasMoreItems && count > 0;

            if (EmptyView != null)
                EmptyView.IsVisible = count == 0 && !_isLoading && !HasMoreItems;
        }

        private void OnCollectionViewScrolled(object? sender, ItemsViewScrolledEventArgs e)
        {
            if (_collectionView == null || _isLoading)
                return;

            int itemCount = GetItemsCount();
            if (itemCount == 0 || e.LastVisibleItemIndex < 0)
                return;

            if (!HasMoreItems)
                return;

            if (e.LastVisibleItemIndex >= itemCount - RemainingItemsThreshold)
                TriggerLoadMore();
        }

        private void OnScrollViewScrolled(object? sender, ScrolledEventArgs e)
        {
            if (_scrollView == null || _isLoading)
                return;

            double scrollY = e.ScrollY;
            double contentHeight = _scrollView.ContentSize.Height;
            double visibleHeight = _scrollView.Height;

            if (scrollY + visibleHeight >= contentHeight - 250)
                TriggerLoadMore();
        }

        private void TriggerLoadMore()
        {
            if (LoadMoreCommand?.CanExecute(null) == true)
            {
                _isLoading = true;
                UpdateViewsVisibility();
                LoadMoreCommand.Execute(null);
                _isLoading = false;
                UpdateViewsVisibility();
                return;
            }
        }

        private ScrollView? FindParentScrollView(VisualElement element)
        {
            Element? parent = element.Parent;
            while (parent != null)
            {
                if (parent is ScrollView scrollView)
                    return scrollView;
                parent = parent.Parent;
            }
            return null;
        }
    }
}
