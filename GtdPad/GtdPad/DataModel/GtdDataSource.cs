using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace GtdPad.Data
{
    /// <summary>
    /// Base class for <see cref="GtdDataItem"/> and <see cref="GtdDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class GtdDataCommon : GtdPad.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public GtdDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(GtdDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class GtdDataItem : GtdDataCommon
    {
        public GtdDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, GtdDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private GtdDataGroup _group;
        public GtdDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class GtdDataGroup : GtdDataCommon
    {
        public GtdDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<GtdDataItem> _items = new ObservableCollection<GtdDataItem>();
        public ObservableCollection<GtdDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<GtdDataItem> _topItem = new ObservableCollection<GtdDataItem>();
        public ObservableCollection<GtdDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// GtdDataSource initializes with placeholder data rather than live production
    /// data so that Gtd data is provided at both design-time and run-time.
    /// </summary>
    public sealed class GtdDataSource
    {
        private static GtdDataSource _gtdDataSource = new GtdDataSource();

        private ObservableCollection<GtdDataGroup> _allGroups = new ObservableCollection<GtdDataGroup>();
        public ObservableCollection<GtdDataGroup> AllGroups
        {
            get { return this._allGroups; }
            set { _allGroups = value; }
        }

        public static IEnumerable<GtdDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _gtdDataSource.AllGroups;
        }

        public static GtdDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _gtdDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static GtdDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _gtdDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public void Save()
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["Things"] = AllGroups;
        }

        public GtdDataSource()
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values.ContainsKey("Things"))
            {
                _gtdDataSource.AllGroups = (ObservableCollection<GtdDataGroup>)roamingSettings.Values["Things"];
            }
            else
            {
                var asap = new GtdDataGroup("ASAP",
                        "ASAP",
                        "As Soon As Possible",
                        "Assets/DarkGray.png",
                        "Things which needs to be done as soon as possible.");

                asap.Items.Add(new GtdDataItem("Work out",
                        "Work out",
                        "",
                        "",
                        "40 pushups",
                        "Do 40 pushups withoud breaks",
                        asap));

                this.AllGroups.Add(asap);


                var waiting = new GtdDataGroup("Waiting",
                        "Waiting",
                        "Waiting for possibility to be done",
                        "Assets/LightGray.png",
                        "List of things which can not be done already. There are waiting for somebody action or some event etc.");

                waiting.Items.Add(new GtdDataItem("Pay Bills",
                        "Pay bills",
                        "",
                        "",
                        "Pay bills for december",
                        "Waiting for recipt.",
                        waiting));

                this.AllGroups.Add(waiting);


                var calendar = new GtdDataGroup("Calendar",
                        "Calendar",
                        "Scheduled things",
                        "Assets/MediumGray.png",
                        "These things should be done in specified terms.");

                calendar.Items.Add(new GtdDataItem("Hairdresser",
                        "Hairdresser",
                        "Friday, 5pm",
                        "",
                        "Go to hairdresser",
                        "Ask for short hairs!",
                        calendar));

                this.AllGroups.Add(calendar);


                var projects = new GtdDataGroup("Projects",
                        "Projects",
                        "Larger affairs",
                        "Assets/LightGray.png",
                        "Category for things which require more than one action.");

                projects.Items.Add(new GtdDataItem("Learn Windows 8",
                        "Learn Windows 8",
                        "",
                        "",
                        "Watch tutorial for total beginners, create 3 store app and publish to Market.",
                        "1. Tutorial for total beginners at Channel 9\n\n 2. dev.windows.com\n\n 3. Create development account\n\n 4. Create 3 games and publish to Market",
                        projects));
                projects.Items.Add(new GtdDataItem("Learn Ruby",
                        "Learn Ruby",
                        "",
                        "",
                        "Learn Ruby language and Ruby on Rails framework for Web Development",
                        "1. Go through Ruby basics\n\n 2. Watch Ruby on Rails tutorial from h:/tutorials/ruby\n\n 3. Create simple internet store app",
                        projects));
                this.AllGroups.Add(projects);


                var future = new GtdDataGroup("FutureMaybe",
                        "Future/Maybe",
                        "Do not know when/if to do",
                        "Assets/MediumGray.png",
                        "Things which will be done in future or not. ");

                future.Items.Add(new GtdDataItem("Start Blog",
                        "Start Blog",
                        "",
                        "",
                        "Start my own technical blog.",
                        "Use Wordpress or DotNetBlogEngine. \n\n Domain: www.awesomeblog.com \n\n Post ideas: Windows 8, Windows Phone 8, Nokia Lumia 920...",
                        future));
                this.AllGroups.Add(future);


                var archive = new GtdDataGroup("Archive",
                        "Archive",
                        "References",
                        "Assets/DarkGray.png",
                        "Things which not require an action. There are stored just for reference.");
                archive.Items.Add(new GtdDataItem("Windows Phone for Absolute beginners",
                        "Windows Phone for Absolute beginners",
                        "",
                        "",
                        "Nice tutorial.",
                        "Created by Bob Tabor. Can be found on Channel 9.",
                        archive));
                this.AllGroups.Add(archive);

                Save();
            }
        }
    }
}
