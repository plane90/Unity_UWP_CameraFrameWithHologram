#if ENABLE_WINMD_SUPPORT
using System;
using System.Threading.Tasks;
using UnityEngine;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Effects;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#endif

public class Test : MonoBehaviour
{

#if ENABLE_WINMD_SUPPORT
        private MediaCaptureVideoProfile _videoProfile;
        private Windows.Media.Capture.Frames.MediaFrameSourceInfo _sourceInfo;
        private Windows.Media.Capture.MediaCapture _mediaCapture;
        private MediaFrameSourceKind sourceKind = MediaFrameSourceKind.Color;
        private MediaStreamType mediaStreamType = MediaStreamType.VideoRecord;


        float elapsed = 0f;
        private void Update()
        {
            if ((elapsed += Time.deltaTime) > 10f)
            {
                gameObject.SetActive(false);
            }
        }

        private async void OnEnable()
        {
            try
            {
                LoggerEx.Log("OnEnable InitVideoSource");
                await InitVideoSource();
                LoggerEx.Log("OnEnable InitMediaCapture");
                await InitMediaCapture();
                LoggerEx.Log("OnEnable StartRecording");
                await StartRecording();
                LoggerEx.Log("OnEnable end");
            }
            catch (Exception exepction)
            {
                LoggerEx.Log(exepction.Message);
            }

        }

        private void OnDisable()
        {
            StopRecording();
        }

        private async Task InitVideoSource()
        {
            try
            {
                LoggerEx.Log("InitVideoSource");
                _videoProfile = await GetVideoProfile();
                if (_videoProfile == null)
                {
                    LoggerEx.Log($"Fail::GetVideoProfile, _videoProfile is null");
                    return;
                }

                foreach (var frameSourceInfo in _videoProfile.FrameSourceInfos)
                {
                    if (frameSourceInfo.SourceKind == sourceKind && frameSourceInfo.MediaStreamType == mediaStreamType)
                    {
                        _sourceInfo = frameSourceInfo;
                    }
                }
            }
            catch (Exception e)
            {
                LoggerEx.Log(e.Message);
            }
        }

        private async Task<MediaCaptureVideoProfile> GetVideoProfile()
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            foreach (var device in devices)
            {
                var videoProfiles = Windows.Media.Capture.MediaCapture.FindKnownVideoProfiles(device.Id, KnownVideoProfile.VideoConferencing);
                if (videoProfiles.Count > 0)
                {
                    return videoProfiles[0];
                }
            }
            return null;
        }

        private async Task InitMediaCapture()
        {
            try
            {
                LoggerEx.Log("InitMediaCapture");
                _mediaCapture = new Windows.Media.Capture.MediaCapture();
                var initSetting = new MediaCaptureInitializationSettings()
                {
                    SourceGroup = _sourceInfo.SourceGroup,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    MediaCategory = MediaCategory.Media,
                    VideoProfile = _videoProfile,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                };
                await _mediaCapture.InitializeAsync(initSetting);
                LoggerEx.Log("MediaCapture Initialized");
            }
            catch (Exception e)
            {
                LoggerEx.Log(e.Message);
            }
        }

        private async Task StartRecording()
        {
            try
            {
                MrcVideoEffectDefinition mrcVideoEffectDefinition = new MrcVideoEffectDefinition();
                var result = await _mediaCapture.AddVideoEffectAsync(mrcVideoEffectDefinition, MediaStreamType.VideoRecord);

                var folder = await Windows.Storage.KnownFolders.GetFolderForUserAsync(null, Windows.Storage.KnownFolderId.CameraRoll);
                var saveFile = await folder.CreateFileAsync("MrcVideo.mp4", Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                if (result == null)
                {
                    Debug.Log("AddVideoEffectAsync Fail");
                }
                LoggerEx.Log("Effect Added !");

                var encoding = Windows.Media.MediaProperties.MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);
                await _mediaCapture.StartRecordToStorageFileAsync(encoding, saveFile);
                LoggerEx.Log("Recording started");
            }
            catch (Exception e)
            {
                LoggerEx.Log(e.Message);
            }
        }

        private async void StopRecording()
        {
            try
            {
                await _mediaCapture.StopRecordAsync();
                LoggerEx.Log("Recording is stoped");
            }
            catch (Exception e)
            {
                LoggerEx.Log(e.Message);
            }
        }

        public sealed class MrcVideoEffectDefinition : IVideoEffectDefinition
        {
            public string ActivatableClassId => "Windows.Media.MixedRealityCapture.MixedRealityCaptureVideoEffect";

            public IPropertySet Properties { get; }

            public MrcVideoEffectDefinition()
            {
                Properties = new PropertySet
                        {
                            {"StreamType", MediaStreamType.VideoRecord},
                            {"HologramCompositionEnabled", true},
                            {"RecordingIndicatorEnabled", true},
                            {"VideoStabilizationEnabled", false},
                            {"VideoStabilizationBufferLength", 0},
                            {"GlobalOpacityCoefficient", 0.9f},
                            {"BlankOnProtectedContent", false},
                            {"ShowHiddenMesh", false},
                            //{"PreferredHologramPerspective", MixedRealityCapturePerspective.PhotoVideoCamera},    // fatal error
                            //{"OutputSize", 0},    // fatal error
                        };
            }
        }
#else
    private void OnEnable()
    {
        Logger.Log("Not supported platform");
    }
#endif
}
