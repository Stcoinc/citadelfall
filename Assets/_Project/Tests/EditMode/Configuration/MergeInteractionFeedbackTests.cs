using ClubGamerZone.TowerDefense.Features.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class MergeInteractionFeedbackTests
    {
        [Test]
        public void AddingFeedback_CreatesReadyTwoDimensionalAudioSource()
        {
            var gameObject = new GameObject("Merge Feedback Test");
            try
            {
                var feedback = gameObject.AddComponent<MergeInteractionFeedback>();
                var audioSource = gameObject.GetComponent<AudioSource>();

                Assert.That(audioSource, Is.Not.Null);
                Assert.That(audioSource.playOnAwake, Is.False);
                Assert.That(audioSource.loop, Is.False);
                Assert.That(audioSource.spatialBlend, Is.Zero);
                Assert.DoesNotThrow(feedback.PlayInvalid);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
