using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace ExtendedTabControl
{
    public class ExtendedTabControl : TabControl
    {
        private RepeatButton tabLeftButton;
        private RepeatButton tabRightButton;
        private Button tabAddItemButton;
        private ScrollViewer tabScrollViewer;
        private TabPanel tabPanelTop;

        public ExtendedTabControl()
        {
            this.DefaultStyleKey = typeof(ExtendedTabControl);
            this.SelectionChanged += OnSelectionChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.tabLeftButton = GetTemplateChild("TabLeftButtonTop") as RepeatButton;
            this.tabRightButton = GetTemplateChild("TabRightButtonTop") as RepeatButton;
            this.tabScrollViewer = GetTemplateChild("TabScrollViewerTop") as ScrollViewer;
            this.tabPanelTop = GetTemplateChild("TabPanelTop") as TabPanel;
            this.tabAddItemButton = GetTemplateChild("TabAddItemButton") as Button;

            if (this.tabLeftButton != null)
                this.tabLeftButton.Click += tabLeftButton_Click;

            if (this.tabRightButton != null)
                this.tabRightButton.Click += tabRightButton_Click;

            if (this.tabAddItemButton != null)
                this.tabAddItemButton.Click += tabAddItemButton_Click;

        }

        #region Add item functionality

        public ICommand AddItemCommand
        {
            get { return (ICommand)GetValue(AddItemCommandProperty); }
            set { SetValue(AddItemCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddItemCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddItemCommandProperty =
            DependencyProperty.Register("AddItemCommand", typeof(ICommand), typeof(ExtendedTabControl), new PropertyMetadata(null));


        void tabAddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.AddItemCommand != null && this.AddItemCommand.CanExecute(null))
                this.AddItemCommand.Execute(null);
        }
        #endregion

        #region Scrollable tabs
        /// <summary>
        /// Gets or sets the Tab Top Left Button style.
        /// </summary>
        /// <value>The left button style style.</value>
        [Description("Gets or sets the tab top left button style")]
        [Category("ScrollButton")]
        public Style TabLeftButtonTopStyle
        {
            get { return (Style)GetValue(TabLeftButtonTopStyleProperty); }
            set { SetValue(TabLeftButtonTopStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Tab Top Right Button style.
        /// </summary>
        /// <value>The left button style style.</value>
        [Description("Gets or sets the tab top right button style")]
        [Category("ScrollButton")]
        public Style TabRightButtonTopStyle
        {
            get { return (Style)GetValue(TabRightButtonTopStyleProperty); }
            set { SetValue(TabRightButtonTopStyleProperty, value); }
        }

        /// <summary>
        /// Tab Top Left Button style
        /// </summary>
        public static readonly DependencyProperty TabLeftButtonTopStyleProperty = DependencyProperty.Register(
            "TabLeftButtonTopStyle",
            typeof(Style),
            typeof(ExtendedTabControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Tab Top Left Button style
        /// </summary>
        public static readonly DependencyProperty TabRightButtonTopStyleProperty = DependencyProperty.Register(
            "TabRightButtonTopStyle",
            typeof(Style),
            typeof(ExtendedTabControl),
            new PropertyMetadata(null));

        //It is the function that is called when the control is resized. I need to update scroll and visibility of buttons
        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            this.UpdateScrollButtonsAvailability();
            //this.ScrollToSelectedItem(); //If the selected item is hidden - be it so
            return size;
        }

        private void tabRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != this.tabScrollViewer && null != this.tabPanelTop)
            {
                //25 pixels to right
                var currentHorizontalOffset = Math.Min(this.tabScrollViewer.HorizontalOffset + 25, this.tabScrollViewer.ScrollableWidth);

                this.tabScrollViewer.ScrollToHorizontalOffset(currentHorizontalOffset);
                UpdateScrollButtonsAvailability(currentHorizontalOffset);
            }
        }
        private void tabLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != this.tabScrollViewer)
            {
                //25 pixels to left
                var currentHorizontalOffset = Math.Max(this.tabScrollViewer.HorizontalOffset - 25, 0);

                this.tabScrollViewer.ScrollToHorizontalOffset(currentHorizontalOffset);
                UpdateScrollButtonsAvailability(currentHorizontalOffset);
            }

        }

        /// <summary>
        /// Change visibility and avalability of buttons if it is necessary
        /// </summary>
        /// <param name="horizontalOffset">the real offset instead of outdated one from the scroll viewer</param>
        private void UpdateScrollButtonsAvailability(double? horizontalOffset = null, double? extraScrollableWidth = null)
        {
            if (this.tabScrollViewer == null) return;

            var hOffset = horizontalOffset ?? this.tabScrollViewer.HorizontalOffset;
            hOffset = Math.Max(hOffset, 0);

            var scrWidth = this.tabScrollViewer.ScrollableWidth - (extraScrollableWidth ?? 0.0);
            scrWidth = Math.Max(scrWidth, 0);

            if (this.tabLeftButton != null)
            {
                this.tabLeftButton.Visibility = scrWidth == 0 ? Visibility.Collapsed : Visibility.Visible;

                this.tabLeftButton.IsEnabled = hOffset > 0;
            }
            if (this.tabRightButton != null)
            {
                var ho = this.tabScrollViewer.HorizontalOffset;
                var w1 = this.tabScrollViewer.ViewportWidth;
                var w2 = this.tabScrollViewer.ScrollableWidth;
                var w3 = this.tabScrollViewer.ExtentWidth;

                this.tabRightButton.Visibility = scrWidth == 0 ? Visibility.Collapsed : Visibility.Visible;

                this.tabRightButton.IsEnabled = hOffset < scrWidth;
            }
        }

        /// <summary>
        /// Scrolls to a selected tab
        /// </summary>
        private void ScrollToSelectedItem()
        {
            var si = base.SelectedItem as TabItem;
            if (si == null || si.ActualWidth == 0 || this.tabScrollViewer == null)
                return;

            var leftItemsWidth = this.Items.Cast<TabItem>().TakeWhile(ti => ti != si).Sum(ti => ti.ActualWidth);

            //If left size + tab size is not visible and situated somwhere at the right area
            if (leftItemsWidth + si.ActualWidth > this.tabScrollViewer.HorizontalOffset + this.tabScrollViewer.ViewportWidth)
            {
                var currentHorizontalOffset = (leftItemsWidth + si.ActualWidth) - (this.tabScrollViewer.HorizontalOffset + this.tabScrollViewer.ViewportWidth);
                var hMargin = this.tabPanelTop.Margin.Left + this.tabPanelTop.Margin.Right;
                currentHorizontalOffset += this.tabScrollViewer.HorizontalOffset + hMargin; //Probably 6 = left margin + right margin


                this.tabScrollViewer.ScrollToHorizontalOffset(currentHorizontalOffset);
                this.UpdateScrollButtonsAvailability(currentHorizontalOffset);
            }
            //if selected item somewhere at the left
            else if (leftItemsWidth < this.tabScrollViewer.HorizontalOffset)
            {
                var currentHorizontalOffset = leftItemsWidth;

                this.tabScrollViewer.ScrollToHorizontalOffset(currentHorizontalOffset);
                this.UpdateScrollButtonsAvailability(currentHorizontalOffset);
            }
        }

        #endregion

        #region Tabs with databinding and templates
        /// <summary>
        /// Gets or sets a DataTemplate for a TabItem header
        /// </summary>
        public new DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public new static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ExtendedTabControl), new PropertyMetadata(
                (sender, e) => ((ExtendedTabControl)sender).InitTabs()));

        /// <summary>
        /// Gets or sets a DataTemplate for a TabItem content
        /// </summary>
        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(ExtendedTabControl), new PropertyMetadata(
                (sender, e) => ((ExtendedTabControl)sender).InitTabs()));

        /// <summary>
        /// Gets or sets a collection used to generate the collection of TabItems
        /// </summary>
        public new IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public new static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ExtendedTabControl), new PropertyMetadata(OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (ExtendedTabControl)sender;
            var incc = e.OldValue as INotifyCollectionChanged;
            if (incc != null)
                incc.CollectionChanged -= control.ItemsSourceCollectionChanged;

            control.InitTabs();

            incc = e.NewValue as INotifyCollectionChanged;
            if (incc != null)
                incc.CollectionChanged += control.ItemsSourceCollectionChanged;
        }

        /// <summary>
        /// Gets or sets the first item in the current selection or returns null if the selection is empty
        /// </summary>
        public new object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public new static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(ExtendedTabControl), new PropertyMetadata(OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (TabControl)sender; //Base class, not extended, because we must change the original SelectedItem property

            if (e.NewValue == null)
                control.SelectedItem = null;
            else
            {
                var tab = control.Items.Cast<TabItem>().FirstOrDefault(ti => ti.DataContext == e.NewValue);
                if (tab != null && control.SelectedItem != tab)
                    control.SelectedItem = tab;
            }
        }

        /// <summary>
        /// Create the collection of TabItems from the collection of clr-objects
        /// </summary>
        private void InitTabs()
        {
            Items.Clear();
            if (this.ItemsSource == null)
                return;

            foreach (var item in ItemsSource)
            {
                var newitem = this.CreateTabItem(item);
                Items.Add(newitem);
            }
        }

        /// <summary>
        /// Creates the TabItem object from a clr-object
        /// </summary>
        /// <param name="dataContext">The clr-object which will be set as the DataContext of the TabItem</param>
        /// <returns>The TabItem object</returns>
        private TabItem CreateTabItem(object dataContext)
        {
            var newitem = new TabItem();

            var hca = new Binding("HorizontalContentAlignment") { Source = this };
            BindingOperations.SetBinding(newitem, Control.HorizontalContentAlignmentProperty, hca);

            var vca = new Binding("VerticalContentAlignment") { Source = this };
            BindingOperations.SetBinding(newitem, Control.VerticalContentAlignmentProperty, vca);

            if (this.ContentTemplate != null)
                newitem.Content = this.ContentTemplate.LoadContent();
            else newitem.Content = dataContext;

            if (this.ItemTemplate != null)
                newitem.Header = this.ItemTemplate.LoadContent();
            else newitem.Header = dataContext;

            newitem.DataContext = dataContext;

            return newitem;
        }

        /// <summary>
        /// Handles the CollectionChanged event of the ItemsSource property
        /// </summary>
        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewStartingIndex > -1)
                {
                    foreach (var item in e.NewItems.OfType<object>().Reverse())
                    {
                        var newitem = this.CreateTabItem(item);
                        Items.Insert(e.NewStartingIndex, newitem);
                        newitem.Loaded += OnNewItemLoaded; //this line must be after the insert, I don't know why  
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldStartingIndex > -1)
                {
                    Items.RemoveAt(e.OldStartingIndex);

                    //Update buttons of scroll
                    this.UpdateScrollButtonsAvailability();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                //I don't know how this action can be called. I would rather ignore it.
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                InitTabs();
            }
        }


        void OnNewItemLoaded(object sender, RoutedEventArgs e)
        {
            var ti = (TabItem)sender;

            if (ti.IsSelected)
                this.ScrollToSelectedItem();
            else //-1 is dirty hack in case if the new item is added, but hasn't received the width
                this.UpdateScrollButtonsAvailability(null, ti.ActualWidth - 1);
        }

        #endregion

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var si = e.AddedItems.Cast<TabItem>().FirstOrDefault();
            if (si != null)
            {
                this.SelectedItem = si.DataContext;
                this.ScrollToSelectedItem();
            }
            else this.SelectedItem = null;
        }

    }

}
