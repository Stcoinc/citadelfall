using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClubGamerZone.TowerDefense.Application.Networking;
using ClubGamerZone.TowerDefense.Application.Authentication;
using ClubGamerZone.TowerDefense.Infrastructure.Firebase;
using NUnit.Framework;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class PersistentAuthenticationTests
    {
        [Test]
        public async Task RestoreSession_RefreshesTokenAndLoadsProfileAndProgression()
        {
            var client = new QueueRestClient(
                new RestResponse(
                    200,
                    "{\"access_token\":\"access\",\"refresh_token\":\"rotated\",\"id_token\":\"identity\",\"user_id\":\"user-1\"}",
                    string.Empty),
                new RestResponse(
                    200,
                    "{\"userId\":\"user-1\",\"username\":\"thornwarden\",\"email\":\"hero@citadelfall.game\",\"avatarId\":\"avatar_04\",\"playerLevel\":1}",
                    string.Empty),
                new RestResponse(
                    200,
                    "{\"experience\":2500,\"level\":3,\"unlockedTowerIds\":[\"tower_archer\"],\"enemyDefeats\":[{\"enemyId\":\"enemy_swarm\",\"defeats\":1000}],\"unlockedCardIds\":[\"enemy_swarm_card_1\"]}",
                    string.Empty));
            var service = new FirebaseAuthenticationService(client, "api-key", "https://example.firebaseio.com");

            var result = await service.RestoreSessionAsync("saved-refresh-token", CancellationToken.None);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Player.Username, Is.EqualTo("thornwarden"));
            Assert.That(result.Player.AvatarId, Is.EqualTo("avatar_04"));
            Assert.That(result.Player.RefreshToken, Is.EqualTo("rotated"));
            Assert.That(result.Player.PlayerLevel, Is.EqualTo(3));
            Assert.That(result.Player.Progression.HasUnlockedTower("tower_archer"), Is.True);
            Assert.That(result.Player.Progression.GetEnemyDefeats("enemy_swarm"), Is.EqualTo(1000));
            Assert.That(client.Requests[0].Body, Does.Contain("grant_type=refresh_token"));
            Assert.That(client.Requests[0].Headers["Content-Type"], Is.EqualTo("application/x-www-form-urlencoded"));
        }

        [Test]
        public async Task ClaimUsername_UsesConditionalCreateAndUpdatesProfile()
        {
            var client = new QueueRestClient(
                new RestResponse(200, "null", string.Empty),
                new RestResponse(200, "{}", string.Empty),
                new RestResponse(200, "{\"userId\":\"user-1\",\"avatarId\":\"avatar_04\",\"playerLevel\":2}", string.Empty),
                new RestResponse(200, "{}", string.Empty));
            var service = new FirebaseAuthenticationService(client, "api-key", "https://example.firebaseio.com");
            var player = new AuthenticatedPlayer("user-1", "hero@citadelfall.game", string.Empty, "avatar_04", "identity", "refresh", false, 2);

            var result = await service.ClaimUsernameAsync(player, "  Thorn_Warden  ", CancellationToken.None);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Player.Username, Is.EqualTo("thorn_warden"));
            Assert.That(client.Requests[1].Method, Is.EqualTo(RestHttpMethod.Put));
            Assert.That(client.Requests[1].Headers["if-match"], Is.EqualTo("null_etag"));
            Assert.That(client.Requests[1].Url, Does.Contain("usernames/thorn_warden.json"));
            Assert.That(client.Requests[3].Body, Does.Contain("\"username\":\"thorn_warden\""));
        }

        [Test]
        public async Task ClaimUsername_WhenConditionalCreateLosesRace_ReturnsTakenAndDoesNotUpdateProfile()
        {
            var client = new QueueRestClient(
                new RestResponse(200, "null", string.Empty),
                new RestResponse(412, "{\"userId\":\"another-user\",\"username\":\"thornwarden\"}", "HTTP/1.1 412 Precondition Failed"));
            var service = new FirebaseAuthenticationService(client, "api-key", "https://example.firebaseio.com");
            var player = new AuthenticatedPlayer("user-1", "hero@citadelfall.game", string.Empty, "avatar_04", "identity", "refresh", false);

            var result = await service.ClaimUsernameAsync(player, "ThornWarden", CancellationToken.None);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already taken"));
            Assert.That(client.Requests, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task ClaimUsername_WhenAlreadySet_RejectsWithoutNetworkRequest()
        {
            var client = new QueueRestClient();
            var service = new FirebaseAuthenticationService(client, "api-key", "https://example.firebaseio.com");
            var player = new AuthenticatedPlayer("user-1", "hero@citadelfall.game", "thornwarden", "avatar_04", "identity", "refresh", false);

            var result = await service.ClaimUsernameAsync(player, "another_name", CancellationToken.None);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already been set"));
            Assert.That(client.Requests, Is.Empty);
        }

        [Test]
        public async Task ClaimUsername_WhenReservationServiceIsUnavailable_ReportsConnectivityInsteadOfTaken()
        {
            var client = new QueueRestClient(
                new RestResponse(200, "null", string.Empty),
                new RestResponse(503, string.Empty, "Service Unavailable"));
            var service = new FirebaseAuthenticationService(client, "api-key", "https://example.firebaseio.com");
            var player = new AuthenticatedPlayer("user-1", "hero@citadelfall.game", string.Empty, "avatar_04", "identity", "refresh", false);

            var result = await service.ClaimUsernameAsync(player, "thornwarden", CancellationToken.None);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("Connect to the internet"));
        }

        private sealed class QueueRestClient : IRestClient
        {
            private readonly Queue<RestResponse> _responses;

            public QueueRestClient(params RestResponse[] responses)
            {
                _responses = new Queue<RestResponse>(responses);
            }

            public List<RestRequest> Requests { get; } = new List<RestRequest>();

            public Task<RestResponse> SendAsync(RestRequest request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Task.FromResult(_responses.Dequeue());
            }
        }
    }
}
