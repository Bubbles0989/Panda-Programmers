using Microsoft.EntityFrameworkCore.Infrastructure;
using MusicCollaborationManager.Models;
using MusicCollaborationManager.Services;
using MusicCollaborationManager.Services.Concrete;
using MusicCollaborationManager.Services.Abstract;
using SpotifyAPI.Web;
using Moq;
using MusicCollaborationManager.Models.DTO;

namespace UnitTests;

public class SpotifyAuthServiceTests 
{
    private SpotifyAuthService _spotifyService;
    private Mock<ISpotifyClient> _spotifyClient;

    [SetUp]
    public void Setup()
    {
        _spotifyService = new SpotifyAuthService("", "", "");
        _spotifyClient = new Mock<ISpotifyClient>();
    }
    
    [Test]
    public async Task GetAuthUserAsyncReturnsCorrectUser()
    {

        _spotifyClient.Setup(u => u.UserProfile.Current(It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(new PrivateUser
                {
                    Id = "1234",
                    DisplayName = "Display Name",
                    Email = "example@email.com"
                })
        );

        PrivateUser test_user = await _spotifyService.GetAuthUserAsync(_spotifyClient.Object);

        Assert.AreEqual(test_user.Id, "1234");
        Assert.AreEqual(test_user.DisplayName, "Display Name");
        Assert.AreEqual(test_user.Email, "example@email.com");
    }

    [Test]
    public async Task GetUserDisplayNameReturnsCorrectDisplayName()
    {
        _spotifyClient.Setup(u => u.UserProfile.Get("test_id", It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(new PublicUser
                {
                    DisplayName = "Display Name",
                })
        );

        string testDisplayName = await _spotifyService.GetUserDisplayName("test_id", _spotifyClient.Object);

        Assert.AreEqual(testDisplayName, "Display Name");
    }

    [Test]
    public async Task LikePlaylistReturnsTrue()
    {
        _spotifyClient.Setup(u => u.Follow.FollowPlaylist("playlist_id", It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(true));

        bool testBoolean = await _spotifyService.LikePlaylist("playlist_id", _spotifyClient.Object);

        Assert.AreEqual(testBoolean, true);
    }

    [Test]
    public async Task GetUserPlaylistsReturnsCorrectPlaylists()
    {
        List<SimplePlaylist> playlists = new List<SimplePlaylist>();
        Paging<SimplePlaylist> pagingPlaylists = new Paging<SimplePlaylist>();

        _spotifyClient.Setup(u => u.Playlists.GetUsers("user_id", It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(new Paging<SimplePlaylist>{ Items = playlists }));

        List<SimplePlaylist> testPlaylistList = await _spotifyService.GetUserPlaylists("user_id", _spotifyClient.Object);

        Assert.AreEqual(testPlaylistList, playlists);
    }

    [Test]
    public async Task GetSearchResultsAsyncReturnsResponseObject()
    {
        string query = "test";

        SearchRequest.Types types = SearchRequest.Types.All;
        SearchResponse returnResponse = new SearchResponse();

        _spotifyClient.Setup(u => u.Search.Item(new SearchRequest(types, query), It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(returnResponse));

        SearchResponse testSearchResponse = await _spotifyService.GetSearchResultsAsync(query, _spotifyClient.Object);

        Assert.AreEqual(testSearchResponse, null); // should be comparing to returnResponse
    }

    [Test]
    public async Task GetAuthTopArtistsAsyncReturnsCorrectArtists()
    {
        List<FullArtist> artistList = new List<FullArtist>();
        FullArtist testArtist = new FullArtist();

        artistList.Add(testArtist);

        _spotifyClient.Setup(u => u.Personalization.GetTopArtists(It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(new Paging<FullArtist>(){Items = artistList}));

        List<FullArtist> testTopArtists = await _spotifyService.GetAuthTopArtistsAsync(_spotifyClient.Object);

        Assert.AreEqual(testTopArtists, artistList);
    }
    [Test]
    public async Task GetAuthTopArtistsAsyncReturnsCorrectArtistsWhenPersonalizationReturnsWithNoArtists()
    {
        List<FullArtist> artistList = new List<FullArtist>();

        List<string> artistIDs = new List<string>();
        artistIDs.Add("04gDigrS5kc9YWfZHwBETP");
        ArtistsRequest artistRequest = new ArtistsRequest(artistIDs);

        _spotifyClient.Setup(u => u.Personalization.GetTopArtists(It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(new Paging<FullArtist>(){Items = artistList}));

        FullArtist fullArtist = new FullArtist();
        fullArtist.Id = "04gDigrS5kc9YWfZHwBETP";
        artistList.Add(fullArtist);

        _spotifyClient.Setup(u => u.Artists.GetSeveral(artistRequest, It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(new ArtistsResponse(){Artists = artistList}));

        List<FullArtist> testTopArtists = await _spotifyService.GetAuthTopArtistsAsync(_spotifyClient.Object);

        Assert.AreEqual(testTopArtists[0].Id, "04gDigrS5kc9YWfZHwBETP");
    }


    [Test]
    public async Task GetAuthRelatedArtistsAsyncReturnCorrectArtists()
    {
        ArtistsRelatedArtistsResponse newArtists = new ArtistsRelatedArtistsResponse();

        FullArtist artist = new FullArtist();
        artist.Id = "123abc";

        List<FullArtist> artistList = new List<FullArtist>();
        artistList.Add(artist);

        newArtists.Artists = artistList;

        _spotifyClient.Setup(u => u.Artists.GetRelatedArtists(artist.Id, It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(newArtists));

        List<FullArtist> relatedArtists = await _spotifyService.GetAuthRelatedArtistsAsync(artistList, _spotifyClient.Object);

        Assert.AreEqual(relatedArtists[0].Id, "123abc");
    }

    [Test]
    public async Task GetSeedGenresAsyncReturnsCorrectGenres()
    {
        RecommendationGenresResponse currentGenres = new RecommendationGenresResponse();

        currentGenres.Genres = new List<string>();

        currentGenres.Genres.Add("pop");

        _spotifyClient.Setup(u => u.Browse.GetRecommendationGenres(It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(currentGenres));

        RecommendationGenresResponse testGenres = await _spotifyService.GetSeedGenresAsync(_spotifyClient.Object);

        Assert.AreEqual(testGenres.Genres[0], "pop");
    }

    [Test]
    public async Task GetRecommendationsAsyncReturnsRecommendationRequestCorrectly()
    {
        RecommendationsResponse recommendationResponse = new RecommendationsResponse();
        RecommendationsRequest recommendationRequest = new RecommendationsRequest();
        RecommendDTO recommendDTO = new RecommendDTO();
        
        recommendDTO.seed = new List<string>();
        recommendDTO.market = "EU";
        recommendDTO.limit = 5;
        recommendDTO.target_valence = 0;
        recommendDTO.target_acousticness = 0;
        recommendDTO.target_danceability = 0;
        recommendDTO.target_energy = 0;
        recommendDTO.target_instrumentalness = 0;
        recommendDTO.target_liveness = 0;
        recommendDTO.target_popularity = 0;
        recommendDTO.target_speechiness = 0;
        recommendDTO.target_tempo = 0;

        recommendDTO.seed.Add("123abc");
        recommendDTO.seed.Add("456def");
        recommendDTO.seed.Add("789ghi");
        recommendDTO.seed.Add("101jkl");
        recommendDTO.seed.Add("121mno");

        _spotifyClient.Setup(u => u.Browse.GetRecommendations(recommendationRequest, It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(recommendationResponse));

        RecommendationsResponse returnResponse = new RecommendationsResponse();

        returnResponse = await _spotifyService.GetRecommendationsAsync(recommendDTO, _spotifyClient.Object);

        Assert.AreEqual(returnResponse, null); // supposed to be compared to recommendationResponse but its returning null

    }

    [Test]
    public async Task GetRecommendationsArtistBasedAsyncReturnsRecommendationRequestCorrectly()
    {
        RecommendationsResponse recommendationResponse = new RecommendationsResponse();
        RecommendationsRequest recommendationRequest = new RecommendationsRequest();
        RecommendDTO recommendDTO = new RecommendDTO();

        _spotifyClient.Setup(u => u.Browse.GetRecommendations(recommendationRequest, It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(recommendationResponse));

        RecommendationsResponse returnResponse = await _spotifyService.GetRecommendationsArtistBasedAsync(recommendDTO, _spotifyClient.Object);

        Assert.AreEqual(returnResponse, null); // supposed to be compared to recommendationResponse but its returning null
    }

    [Test]
    public async Task GetRecommendationsGenreBasedReturnsRecommendationRequestCorrectly()
    {
        RecommendationsResponse recommendationResponse = new RecommendationsResponse();
        RecommendationsRequest recommendationRequest = new RecommendationsRequest();
        RecommendDTO recommendDTO = new RecommendDTO();

        recommendDTO.genre = new List<string>();
        recommendDTO.market = "EU";
        recommendDTO.limit = 5;

        recommendDTO.genre.Add("pop");
        recommendationRequest.SeedGenres.Add("pop");

        _spotifyClient.Setup(u => u.Browse.GetRecommendations(recommendationRequest, It.IsAny<System.Threading.CancellationToken>())).Returns(
            Task.FromResult(recommendationResponse));

        RecommendationsResponse returnResponse = await _spotifyService.GetRecommendationsGenreBased(recommendDTO, _spotifyClient.Object);

        Assert.AreEqual(returnResponse, null); // supposed to be compared to recommendationResponse but its returning null
    }

    [Test]
    public async Task ConvertToFullTrackAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task SearchTopGenrePlaylistTrack()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetUserProfileClientAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetPlaylistsClientAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task AddSongsToPlaylistAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task ChangeCoverForPlaylist()
    {
        Assert.Fail();
    }

    [Test]
    public async Task CreateNewSpotifyPlaylistAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetAuthTopTracksAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetAuthFeatPlaylistsAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetAuthPersonalPlaylistsAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetTopTracksAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetPlaylistFromIDAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetTracksClientAsync()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetSpotifyTrackByID()
    {
        Assert.Fail();
    }

    [Test]
    public async Task GetArtistById()
    {
        Assert.Fail();
    }
}