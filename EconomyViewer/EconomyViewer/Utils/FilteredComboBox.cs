using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EconomyViewer.Utils
{
    public class FilteredComboBox : ComboBox
    { /// <summary>
      /// The search string treshold length.
      /// </summary>
      /// <remarks>
      /// It's implemented as a Dependency Property, so you can set it in a XAML template
      /// </remarks>
        public static readonly DependencyProperty MinimumSearchLengthProperty =
            DependencyProperty.Register(
                "MinimumSearchLength",
                typeof(int),
                typeof(FilteredComboBox),
                new UIPropertyMetadata(int.MaxValue));

        /// <summary>
        /// Holds the current value of the filter.
        /// </summary>
        private string currentFilter = "";
        /// <summary>
        /// Caches the previous value of the filter.
        /// </summary>
        private string oldFilter = "";
        /// <summary>
        /// Gets or sets the search string treshold length.
        /// </summary>
        /// <value>The minimum length of the search string that triggers filtering.</value>
        [Description("Length of the search string that triggers filtering.")]
        [Category("Filtered ComboBox")]
        [DefaultValue(int.MaxValue)]
        public int MinimumSearchLength
        {
            [DebuggerStepThrough]
            get => (int)GetValue(MinimumSearchLengthProperty);

            [DebuggerStepThrough]
            set => SetValue(MinimumSearchLengthProperty, value);
        }
        /// <summary>
        /// Gets a reference to the internal editable textbox.
        /// </summary>
        /// <value>A reference to the internal editable textbox.</value>
        /// <remarks>
        /// We need this to get access to the Selection.
        /// </remarks>
        protected TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;
        /// <summary>
        /// Initializes a new instance of the FilteredComboBox class.
        /// </summary>
        /// <remarks>
        /// You could set 'IsTextSearchEnabled' to 'false' here,
        /// to avoid non-intuitive behavior of the control
        /// </remarks>
        public FilteredComboBox()
        {

        }

        /// <summary>
        /// Keep the filter if the ItemsSource is explicitly changed.
        /// </summary>
        /// <param name="oldValue">The previous value of the filter.</param>
        /// <param name="newValue">The current value of the filter.</param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(newValue);
                view.Filter += FilterPredicate;
            }

            if (oldValue != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(oldValue);
                view.Filter -= FilterPredicate;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            if (Text == "")
            {
                RefreshFilter();
            }
            base.OnDropDownOpened(e);
        }
        /// <summary>
        /// Modify and apply the filter.
        /// </summary>
        /// <param name="e">Key Event Args.</param>
        /// <remarks>
        /// Alternatively, you could react on 'OnTextChanged', but navigating through
        /// the DropDown will also change the text.
        /// </remarks>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if ((e.Key == Key.Up || e.Key == Key.Down) && SelectedIndex != -1) { }
            else if (e.Key == Key.Tab || e.Key == Key.Enter)
            {
                ClearFilter();
            }
            else
            {
                if (Text != oldFilter)
                {
                    if (Text.Length != 0)
                    {
                        RefreshFilter();
                        IsDropDownOpen = true;
                        EditableTextBox.SelectionStart = int.MaxValue;
                    }
                    else
                    {
                        IsDropDownOpen = false;
                        RefreshFilter();
                        SelectedIndex = -1;
                    }
                    MaxDropDownHeight = Items.Count == 0 ? 0 : 350;

                }
                base.OnKeyUp(e);
                currentFilter = Text;
            }
        }
        /// <summary>
        /// Confirm or cancel the selection when Tab, Enter, or Escape are hit.
        /// Open the DropDown when the Down Arrow is hit.
        /// </summary>
        /// <param name="e">Key Event Args.</param>
        /// <remarks>
        /// The 'KeyDown' event is not raised for Arrows, Tab and Enter keys.
        /// It is swallowed by the DropDown if it's open.
        /// So use the Preview instead.
        /// </remarks>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter)
                IsDropDownOpen = false;
            else if (e.Key == Key.Escape)
            {
                IsDropDownOpen = false;
                SelectedIndex = -1;
                Text = currentFilter;
            }
            else if (e.Key == Key.Back && EditableTextBox.SelectedText.Length == Text.Length)
            {
                RefreshFilter();
            }
            else
            {
                if (e.Key == Key.Down)
                    IsDropDownOpen = true;
                base.OnPreviewKeyDown(e);
            }
            oldFilter = Text;
        }
        /// <summary>
        /// Make sure the text corresponds to the selection when leaving the control.
        /// </summary>
        /// <param name="e">A KeyBoardFocusChangedEventArgs.</param>
        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            ClearFilter();
            int temp = SelectedIndex;
            SelectedIndex = -1;
            Text = "";
            SelectedIndex = temp;
            base.OnPreviewLostKeyboardFocus(e);
        }
        /// <summary>
        /// Clear the Filter.
        /// </summary>
        private void ClearFilter()
        {
            currentFilter = "";
            RefreshFilter();
        }
        /// <summary>
        /// The Filter predicate that will be applied to each row in the ItemsSource.
        /// </summary>
        /// <param name="value">A row in the ItemsSource.</param>
        /// <returns>Whether or not the item will appear in the DropDown.</returns>
        private bool FilterPredicate(object value)
        {
            if (value == null)
                return false;

            if (Text.Length == 0)
                return true;

            return value.ToString().ToLower().Contains(Text.ToLower());
        }
        /// <summary>
        /// Re-apply the Filter.
        /// </summary>
        private void RefreshFilter()
        {
            if (ItemsSource != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
                view.Refresh();
            }
        }
    }
}
