﻿using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ContosoMoments.ViewModels
{
    public class AlbumsListViewModel : BaseViewModel, IDisposable
    {
        private IDisposable eventSubscription;

        public AlbumsListViewModel(MobileServiceClient client, App app)
        {
            this.app = app;

            RenameCommand = new DelegateCommand(OnStartAlbumRename, IsRenameAndDeleteEnabled);
            DeleteCommand = new DelegateCommand(OnDeleteAlbum, IsRenameAndDeleteEnabled);

            eventSubscription = client.EventManager.Subscribe<SyncCompletedEvent>(OnSyncCompleted);
        }

        #region View Model Properties
        private string _ErrorMessage = null;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                _ErrorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public string ErrorMessageTitle { get; set; }

        App app;
        private Album currentAlbumEdit;

        private string editedName;

        public string EditedAlbumName
        {
            get { return editedName; }
            set
            {
                editedName = value;
                OnPropertyChanged(nameof(EditedAlbumName));
            }
        }

        private bool showCancelButton;

        public bool ShowCancelButton
        {
            get { return showCancelButton && showInputControl; }
            set
            {
                showCancelButton = value;
                OnPropertyChanged(nameof(ShowCancelButton));
            }
        }

        public string CreateOrUpdateButtonText
        {
            get { return isRename ? "Rename" : "Add"; }
        }

        private bool isRename;

        public bool IsRename
        {
            get { return isRename; }
            set
            {
                isRename = value;
                OnPropertyChanged(nameof(IsRename));
                OnPropertyChanged(nameof(CreateOrUpdateButtonText));
            }
        }

        private bool showInputControl;

        public bool ShowInputControl
        {
            get { return showInputControl; }
            set
            {
                showInputControl = value;
                OnPropertyChanged(nameof(ShowInputControl));
                OnPropertyChanged(nameof(ShowCancelButton));
            }
        }

        private List<Album> albums;
        public List<Album> Albums
        {
            get { return albums; }
            set
            {
                albums = value;
                OnPropertyChanged(nameof(Albums));
            }
        }

        public ICommand RenameCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        // called when the album delete button is clicked
        public Action<Album> DeleteAlbumViewAction { get; set; }

        #endregion

        public async Task CheckUpdateNotificationRegistrationAsync(string userId)
        {
#if !__WP__
            string installationId = App.Instance.MobileService.GetPush().InstallationId;
#elif (__WP__ && DEBUG)
            string installationId = "8a526c49-b824-4a81-8f27-dce0e383e850";
#endif

#if (!__WP__) || (__WP__ && DEBUG)
            var jsonRequest = new JObject();
            jsonRequest["InstallationId"] = installationId;
            jsonRequest["UserId"] = userId;

            await App.Instance.MobileService.InvokeApiAsync("PushRegistration", jsonRequest, HttpMethod.Post, null);
#endif
        }

        private async void OnSyncCompleted(SyncCompletedEvent obj)
        {
            await LoadItemsAsync(Settings.Current.CurrentUserId);
        }

        public async Task LoadItemsAsync(string userId)
        {
            Albums = 
                await app.albumTableSync
                    .OrderBy(x => x.CreatedAt)
                    .ToListAsync();
        }

        #region UI Actions
        private void OnStartAlbumRename(object obj)
        {
            var selectedAlbum = obj as Album;
            Debug.WriteLine($"Selected album: {selectedAlbum?.AlbumName}");

            if (selectedAlbum != null) {
                currentAlbumEdit = selectedAlbum;
                IsRename = true;
                EditedAlbumName = selectedAlbum.AlbumName;
                ShowInputControl = true;
                ShowCancelButton = true;
            }
        }

        public async Task DeleteAlbumAsync(Album selectedAlbum)
        {
            await app.albumTableSync.DeleteAsync(selectedAlbum);

            DependencyService.Get<IPlatform>().LogEvent("DeleteAlbum");
        }

        private void OnDeleteAlbum(object obj)
        {
            var selectedAlbum = obj as Album;
            DeleteAlbumViewAction?.Invoke(selectedAlbum);
        }

        public async Task<bool> CreateOrRenameAlbum()
        {
            if (currentAlbumEdit == null || EditedAlbumName.Length > 0) {
                ShowInputControl = false;

                if (IsRename) {
                    currentAlbumEdit.AlbumName = EditedAlbumName;
                    await app.albumTableSync.UpdateAsync(currentAlbumEdit);
                }
                else {
                    await CreateAlbumAsync();
                }

                return true;
            }

            return false;
        }

        private async Task CreateAlbumAsync()
        {
            var album = new Album() 
            {
                AlbumName = EditedAlbumName,
                IsDefault = false,
                UserId = Settings.Current.CurrentUserId
            };

            await app.albumTableSync.InsertAsync(album);

            DependencyService.Get<IPlatform>().LogEvent("CreateAlbum");
        }

        public void AddImage()
        {
            ShowInputControl = !ShowInputControl;
            IsRename = false;
            ShowCancelButton = false;
        }

        #endregion

        // return true if the rename and delete commands are available
        internal static bool IsRenameAndDeleteEnabled(object input)
        {
            var album = input as Album;
            var isDefaultAlbum = album != null ? album.IsDefault : false;  // default album can't be renamed

            return !isDefaultAlbum && Settings.Current.AuthenticationType != Settings.AuthOption.GuestAccess;
        }
        
        public void Dispose()
        {
            eventSubscription.Dispose();
        }

    }

}

