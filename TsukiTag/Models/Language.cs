using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Extensions;

namespace TsukiTag.Models
{
    public class Language
    {
        private static ILocalizer localizer;

        public static ILocalizer Localizer
        {
            get
            {
                if (localizer == null)
                {
                    localizer = Ioc.SimpleIoc.Localizer;
                }

                return localizer;
            }
        }

        public Language()
        { }

        public static string Providers => Localizer.Get(nameof(Providers));
        public static string AllTags => Localizer.Get(nameof(AllTags));
        public static string Metadata => Localizer.Get(nameof(Metadata));
        public static string Name => Localizer.Get(nameof(Name));
        public static string All => Localizer.Get(nameof(All));
        public static string Session => Localizer.Get(nameof(Session));
        public static string Refresh => Localizer.Get(nameof(Refresh));

        public static string Safe => Localizer.Get(nameof(Safe));
        public static string Questionable => Localizer.Get(nameof(Questionable));
        public static string Explicit => Localizer.Get(nameof(Explicit));

        public static string NextPage => Localizer.Get(nameof(NextPage));
        public static string PreviousPage => Localizer.Get(nameof(PreviousPage));

        public static string Browse => Localizer.Get(nameof(Browse));

        public static string OnlineLists => Localizer.Get(nameof(OnlineLists));
        public static string OnlineList => Localizer.Get(nameof(OnlineList));
        public static string OnlineListsDescription => Localizer.Get(nameof(OnlineListsDescription));
        public static string ListIsDefault => Localizer.Get(nameof(ListIsDefault));
        public static string ListRemove => Localizer.Get(nameof(ListRemove));
        public static string ListAdd => Localizer.Get(nameof(ListAdd));
        public static string ListTagsToAdd => Localizer.Get(nameof(ListTagsToAdd));
        public static string ListTagsToAddDescription => Localizer.Get(nameof(ListTagsToAddDescription));
        public static string ListTagsToRemove => Localizer.Get(nameof(ListTagsToRemove));
        public static string ListTagsToRemoveDescription => Localizer.Get(nameof(ListTagsToRemoveDescription));
        public static string ListTagsNew => Localizer.Get(nameof(ListTagsNew));
        public static string ListTagsRemove => Localizer.Get(nameof(ListTagsRemove));
        public static string ListOptionalConditionTag => Localizer.Get(nameof(ListOptionalConditionTag));
        public static string ListOptionalConditionTagDescription => Localizer.Get(nameof(ListOptionalConditionTagDescription));
        public static string ListMandatoryConditionTag => Localizer.Get(nameof(ListMandatoryConditionTag));
        public static string ListMandatoryConditionTagDescription => Localizer.Get(nameof(ListMandatoryConditionTagDescription));
        public static string List => Localizer.Get(nameof(List));
        public static string ListFavorite => Localizer.Get(nameof(ListFavorite));
        public static string TemplateDescription => Localizer.Get(nameof(TemplateDescription));
        public static string PictureTemplateHelp => $"{Localizer.Get(nameof(TemplateDescription))}{typeof(Picture).GetPropertyTemplateNameIteration()}";
        public static string SettingsSave => Localizer.Get(nameof(SettingsSave));
        public static string SettingsCancel => Localizer.Get(nameof(SettingsCancel));
        public static string SettingsListNoName => Localizer.Get(nameof(SettingsListNoName));
        public static string SettingsListNotUnique => Localizer.Get(nameof(SettingsListNotUnique));
        public static string SettingsSaved => Localizer.Get(nameof(SettingsSaved));
        public static string SettingsSaveError => Localizer.Get(nameof(SettingsSaveError));

        public static string ActionOnlineLists => Localizer.Get(nameof(ActionOnlineLists));
        public static string ActionAddToDefault => Localizer.Get(nameof(ActionAddToDefault));
        public static string ActionAddToEligible => Localizer.Get(nameof(ActionAddToEligible));
        public static string ActionAddTo => Localizer.Get(nameof(ActionAddTo));
        public static string ActionRemoveFromAll => Localizer.Get(nameof(ActionRemoveFromAll));
        public static string ActionRemoveFrom => Localizer.Get(nameof(ActionRemoveFrom));
        public static string ActionGenericError => Localizer.Get(nameof(ActionGenericError));
        public static string ActionRemoveFromAllSuccess => Localizer.Get(nameof(ActionRemoveFromAllSuccess));
        public static string ActionRemoveFromSuccess => Localizer.Get(nameof(ActionRemoveFromSuccess));
        public static string ActionAddToAllSuccess => Localizer.Get(nameof(ActionAddToAllSuccess));
        public static string ActionAddToSuccess => Localizer.Get(nameof(ActionAddToSuccess));
        public static string ActionAddToEligibleSuccess => Localizer.Get(nameof(ActionAddToEligibleSuccess));
        public static string ActionThisImage => Localizer.Get(nameof(ActionThisImage));
        public static string ActionSelection => Localizer.Get(nameof(ActionSelection));
        public static string ActionSelectionActionSuccess => Localizer.Get(nameof(ActionSelectionActionSuccess));


        public static string PreviousPicture => Localizer.Get(nameof(PreviousPicture));
        public static string NextPicture => Localizer.Get(nameof(NextPicture));
        public static string OpenPicture => Localizer.Get(nameof(OpenPicture));
        public static string DeselectPicture => Localizer.Get(nameof(DeselectPicture));
        public static string DeselectAllPictures => Localizer.Get(nameof(DeselectAllPictures));
        public static string SwitchToTag => Localizer.Get(nameof(SwitchToTag));
        public static string SwitchToMetadata => Localizer.Get(nameof(SwitchToMetadata));
        public static string OriginalView => Localizer.Get(nameof(OriginalView));
        public static string ClosePicture => Localizer.Get(nameof(ClosePicture));
        public static string AddThisTagSearch => Localizer.Get(nameof(AddThisTagSearch));
        public static string RemoveThisTagSearch => Localizer.Get(nameof(RemoveThisTagSearch));
        public static string SetThisTagSearch => Localizer.Get(nameof(SetThisTagSearch));

        public static string ToastProviderEnd => Localizer.Get(nameof(ToastProviderEnd));
        public static string ToastOnlineListEnd => Localizer.Get(nameof(ToastOnlineListEnd));
        public static string ToastOnlineListsEnd => Localizer.Get(nameof(ToastOnlineListsEnd));
        public static string ToastProviderError => Localizer.Get(nameof(ToastProviderError));
        public static string ToastProviderTagLimit2 => Localizer.Get(nameof(ToastProviderTagLimit2));

        public static string NavigationOnline => Localizer.Get(nameof(NavigationOnline));
        public static string NavigationSettings => Localizer.Get(nameof(NavigationSettings));
        public static string NavigationAllOnlineLists => Localizer.Get(nameof(NavigationAllOnlineLists));
        public static string NavigationSpecificOnlineList => Localizer.Get(nameof(NavigationSpecificOnlineList));
    }
}
