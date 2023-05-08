#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace LottiePlugin.UI.Editor
{
    [CustomEditor(typeof(AnimatedImage), true)]
    [CanEditMultipleObjects]
    internal sealed class AnimatedImageEditor : UnityEditor.Editor
    {
        //Own
        private SerializedProperty _animationJsonProperty;
        private SerializedProperty _animationSpeedProperty;
        private SerializedProperty _widthProperty;
        private SerializedProperty _heightProperty;
        private SerializedProperty _playOnAwake;
        private SerializedProperty _loop;

        private LottieAnimation _lottieAnimation;
        private string _animationInfoBoxText;

        private void OnEnable()
        {
            _animationJsonProperty = serializedObject.FindProperty("_animationJson");
            _animationSpeedProperty = serializedObject.FindProperty("_animationSpeed");
            _widthProperty = serializedObject.FindProperty("_textureWidth");
            _heightProperty = serializedObject.FindProperty("_textureHeight");
            _playOnAwake = serializedObject.FindProperty("_playOnAwake");
            _loop = serializedObject.FindProperty("_loop");

            CreateAnimationIfNecessaryAndAttachToGraphic();
            UpdateTheAnimationInfoBoxText();
        }
        private void OnDisable()
        {
            _lottieAnimation?.Dispose();
            _lottieAnimation = null;
            SetGraphicsTexture(null);
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            AnimatedImage image = serializedObject.targetObject as AnimatedImage;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_animationJsonProperty);
            if (EditorGUI.EndChangeCheck())
            {
                _lottieAnimation?.Dispose();
                _lottieAnimation = null;
                CreateAnimationIfNecessaryAndAttachToGraphic();
                UpdateTheAnimationInfoBoxText();
            }
            if (image.AnimationJson == null ||
                string.IsNullOrEmpty(image.AnimationJson.text) ||
                !image.AnimationJson.text.StartsWith("{\"v\":"))
            {
                EditorGUILayout.HelpBox("You must have a lottie json in order to use the animated image.", MessageType.Error);
            }
            if (_lottieAnimation != null)
            {
                EditorGUILayout.HelpBox(_animationInfoBoxText, MessageType.Info);
            }
            EditorGUILayout.Space();
            if (_widthProperty.intValue == 0)
            {
                _widthProperty.intValue = 128;
            }
            if (_heightProperty.intValue == 0)
            {
                _heightProperty.intValue = 128;
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_animationSpeedProperty);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateTheAnimationInfoBoxText();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_widthProperty);
            EditorGUILayout.PropertyField(_heightProperty);
            if (EditorGUI.EndChangeCheck())
            {
                _lottieAnimation?.Dispose();
                _lottieAnimation = null;
                CreateAnimationIfNecessaryAndAttachToGraphic();
            }
            EditorGUILayout.EndHorizontal();
            if (_widthProperty.intValue > 2048 || _heightProperty.intValue > 2048)
            {
                EditorGUILayout.HelpBox("Higher texture resolution will consume more processor resources at runtime.", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(_playOnAwake);
            EditorGUILayout.PropertyField(_loop);
            serializedObject.ApplyModifiedProperties();
        }
        private void CreateAnimationIfNecessaryAndAttachToGraphic()
        {
            if (_lottieAnimation != null)
            {
                return;
            }
            serializedObject.ApplyModifiedProperties();
            AnimatedImage image = serializedObject.targetObject as AnimatedImage;
            if (image.AnimationJson == null)
            {
                return;
            }
            string jsonData = image.AnimationJson.text;
            if (string.IsNullOrEmpty(jsonData) ||
                !jsonData.StartsWith("{\"v\":"))
            {
                Debug.LogError("Selected file is not a lottie json");
                return;
            }
            _lottieAnimation = LottieAnimation.LoadFromJsonData(
                jsonData,
                string.Empty,
                image.TextureWidth,
                image.TextureHeight);
            _lottieAnimation.DrawOneFrame(0);
            SetGraphicsTexture(_lottieAnimation.Texture);
        }
        private void UpdateTheAnimationInfoBoxText()
        {
            if (_lottieAnimation == null)
            {
                return;
            }
            _animationInfoBoxText = $"Animation info: Frame Rate \"{_lottieAnimation.FrameRate.ToString("F2")}\", " +
                    $"Total Frames \"{_lottieAnimation.TotalFramesCount.ToString()}\", " +
                    $"Original Duration \"{_lottieAnimation.DurationSeconds.ToString("F2")}\" sec. " +
                    $"Play Duration \"{(_lottieAnimation.DurationSeconds / _animationSpeedProperty.floatValue).ToString("F2")}\" sec. ";
        }
        private void SetGraphicsTexture(Texture2D texture)
        {
            AnimatedImage image = serializedObject.targetObject as AnimatedImage;
            if (image == null)
            {
                return;
            }
            if (image.RawImage == null)
            {
                image.RawImage = image.GetComponent<RawImage>();
            }
            image.RawImage.texture = texture;
        }
    }
}
#endif