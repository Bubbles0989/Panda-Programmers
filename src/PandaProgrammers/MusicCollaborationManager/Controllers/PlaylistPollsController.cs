﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicCollaborationManager.DAL.Abstract;
using MusicCollaborationManager.Models;
using MusicCollaborationManager.Models.DTO;
using MusicCollaborationManager.Services.Abstract;
using MusicCollaborationManager.Services.Concrete;
using SpotifyAPI.Web;

namespace MusicCollaborationManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistPollsController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IListenerRepository _listenerRepository;
        private readonly SpotifyAuthService _spotifyService;
        private readonly IPollsService _pollsService;
        private readonly IPlaylistPollRepository _playlistPollRepository;

        public PlaylistPollsController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, SpotifyAuthService spotifyService, IListenerRepository listenerRepository, 
            IPollsService pollsService, IPlaylistPollRepository playlistPollRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _spotifyService = spotifyService;
            _listenerRepository = listenerRepository;
            _pollsService = pollsService;
            _playlistPollRepository = playlistPollRepository;
        }


        [HttpGet("checkifpollexists/{username}/{playlistid}")]
        public async Task<GeneralPollInfoDTO> CheckIfPollExists(string username, string playlistid) 
        {
            GeneralPollInfoDTO PotentialNewPoll = new GeneralPollInfoDTO();

                Poll? NewPoll = _playlistPollRepository.GetPollDetailsBySpotifyPlaylistID(playlistid);
            
            if (NewPoll == null)
            {

                //string NewPollID = await _pollsService.CreatePollForSpecificPlaylist(playlistId);
                //string NewPollID = await _pollsService.CreatePollForSpecificPlaylist(newPollInput.PlaylistID);


                //Debugging (below)------------
                PotentialNewPoll.TrackArtist = playlistid + "_PlaylistID";
                PotentialNewPoll.TrackTitle = "TrackID_" + username + "___Created_by_'checkifpollexists'";
                PotentialNewPoll.TrackDuration = "4 MIN";
                PotentialNewPoll.YesOptionID = "#1234_YES";
                PotentialNewPoll.NoOptionID = "#5678_NO";
                PotentialNewPoll.TotalPollVotes = "MADEUP_3";
                PotentialNewPoll.PlaylistFollowerCount = "MADEUP_4";
                PotentialNewPoll.UserVotedYes = true;
                //PotentialNewPoll.UserVotedYes = false;
               
                return PotentialNewPoll;
            }
            else
            {
                PotentialNewPoll.TrackArtist = playlistid + "_PlaylistID";
                PotentialNewPoll.TrackTitle = "TrackID_" + username + "___Created_by_'checkifpollexists'";
                PotentialNewPoll.TrackDuration = "4 MIN";
                PotentialNewPoll.YesOptionID = "#1234_YES";
                PotentialNewPoll.NoOptionID = "#5678_NO";
                PotentialNewPoll.TotalPollVotes = "4";
                PotentialNewPoll.PlaylistFollowerCount = "EMPTY_ON_PURPOSE";
                PotentialNewPoll.UserVotedYes = null; //User has NOT VOTED.

                return PotentialNewPoll;
            }
        }



        [HttpPost("createpoll")]
        public async Task<GeneralPollInfoDTO> CreateNewPoll([Bind("NewPollPlaylistId,NewPollTrackId")] PollCreationDTO newPollInput) //TrackID passed here (instead of "trackuri"). (Just haven't updated the name in DB yet.)
        {
                GeneralPollInfoDTO PotentialNewPoll = new GeneralPollInfoDTO();

                Poll? NewPoll = _playlistPollRepository.GetPollDetailsBySpotifyPlaylistID(newPollInput.NewPollPlaylistId);
                if (NewPoll == null)
                {

                    //string NewPollID = await _pollsService.CreatePollForSpecificPlaylist(playlistId);
                    //string NewPollID = await _pollsService.CreatePollForSpecificPlaylist(newPollInput.PlaylistID);


                    //Debugging (below)------------
                    PotentialNewPoll.TrackArtist = newPollInput.NewPollPlaylistId + "_PlaylistID";
                    PotentialNewPoll.TrackTitle = "TrackID" + newPollInput.NewPollTrackId + "___Created_by_'createpoll'";
                    PotentialNewPoll.TrackDuration = "4 MIN";
                    PotentialNewPoll.YesOptionID = "#1234_YES";
                    PotentialNewPoll.NoOptionID = "#5678_NO";
                    PotentialNewPoll.UserVotedYes = true;

                //Original
                PotentialNewPoll.TotalPollVotes = "1";
                PotentialNewPoll.PlaylistFollowerCount = "5";

                //Case where totalpollvotes is higher than follower count.
                //PotentialNewPoll.TotalPollVotes = "2";
                //PotentialNewPoll.PlaylistFollowerCount = "3";


                return PotentialNewPoll;
                    

                    //Debugging (Above)-----------

                    //NewPoll.PollId = NewPollID;
                    //NewPoll.SpotifyPlaylistId = newPollInput.PlaylistID;
                    //NewPoll.SpotifyTrackUri = newPollInput.TrackID;
              
                    //_playlistPollRepository.AddOrUpdate(NewPoll);
                    //IEnumerable<OptionInfoDTO> PollOptions = await _pollsService.GetPollOptionsByPollID(NewPollID);

                    //FullTrack TrackBeingPolled = await _spotifyService.GetSpotifyTrackByID(newPollInput.TrackID, SpotifyAuthService.GetTracksClientAsync());
                    //PotentialNewPoll.TrackArtist = TrackBeingPolled.Artists[0].Name;
                    //PotentialNewPoll.TrackTitle = TrackBeingPolled.Name;
                    //PotentialNewPoll.TrackDuration = TrackBeingPolled.DurationMs.ToString(); //Convert to minutes later if possible.

                    //foreach(var option in PollOptions) 
                    //{
                    //    if(option.OptionText == "Yes") 
                    //    {
                    //        PotentialNewPoll.YesOptionID = option.OptionID;
                    //    }
                    //    if(option.OptionText == "No") 
                    //    {
                    //        PotentialNewPoll.NoOptionID = option.OptionID;
                    //    }
                    //    PotentialNewPoll.TotalPollVotes += option.OptionCount;
                    //}
                    //return PotentialNewPoll;
                }
                return null;
        }


        //Old version (below)---

        ////NEEDS TESTING
        //[HttpPost("createpoll/{playlistid}/{trackuri}")]
        //public async Task<IEnumerable<OptionInfoDTO>> CreateNewPoll(string playlistid, string trackuri) //TrackID passed here (instead of "trackuri"). (Just haven't update the name in DB yet.)
        //{
        //    try 
        //    {
        //        Poll? NewPoll = _playlistPollRepository.GetPollDetailsBySpotifyPlaylistID(playlistid);
        //        if (NewPoll == null) 
        //        {
        //            string? NewPollID = await _pollsService.CreatePollForSpecificPlaylist(playlistid);

        //            NewPoll.PollId = NewPollID;
        //            NewPoll.SpotifyPlaylistId = playlistid;
        //            NewPoll.SpotifyTrackUri = trackuri;

        //            _playlistPollRepository.AddOrUpdate(NewPoll);
        //            return await _pollsService.GetPollOptionsByPollID(NewPollID);
        //        }
        //        return Enumerable.Empty<OptionInfoDTO>();

        //    }
        //    catch (Exception ex) 
        //    {
        //        return Enumerable.Empty<OptionInfoDTO>();
        //    }
        // }

        //Should give user option for enabling a vote AND cover the possibility of the poll ending HERE.
        //[HttpPost("createvote")]
        //public async Task<bool?> CreateVoteOnExistingPoll(string playlistid, string optionID, string username)
        //{
        //    bool? trackAddedToPlaylist = false;
        //    try 
        //    {
        //        Poll PollInfo = _playlistPollRepository.GetPollDetailsBySpotifyPlaylistID(playlistid);
        //        IEnumerable<OptionInfoDTO> PollOptions = await _pollsService.GetPollOptionsByPollID(PollInfo.PollId);
        //        await _pollsService.CreateVoteForTrack(PollInfo.PollId, optionID, username);


        //        //NEED LOGIC HERE TO CHECK IF NUM FOLLOWERS IS EQUAL TO NUM VOTES. (If so, END the poll).
        //        FullPlaylist CurPlaylist = await _spotifyService.GetPlaylistFromIDAsync(playlistid);


        //        if(CurPlaylist.Followers.Total == PollOptions.ToList().Count()) 
        //        {
        //            int yesCount = 0;
        //            int noCount = 0;
        //            foreach(var pollingoption in PollOptions) 
        //            {
        //                if(pollingoption.OptionText == "Yes") 
        //                {
        //                    yesCount = pollingoption.OptionCount;
        //                }
        //                else if(pollingoption.OptionText == "No") 
        //                {
        //                    noCount = pollingoption.OptionCount;
        //                }
        //            }
        //            if (yesCount > noCount)
        //            {
        //                FullTrack TrackBeingVoted = await _spotifyService.GetSpotifyTrackByID(PollInfo.SpotifyTrackUri, SpotifyAuthService.GetTracksClientAsync());
        //                List<string> TrackToAddAsList = new List<string>
        //                {
        //                    PollInfo.SpotifyTrackUri
        //                };

        //                await _spotifyService.AddSongsToPlaylistAsync(CurPlaylist, TrackToAddAsList);
        //                trackAddedToPlaylist = true;
        //            }
        //            await _pollsService.RemovePoll(playlistid);
        //            _playlistPollRepository.Delete(PollInfo);
        //        }
        //        return trackAddedToPlaylist;
        //    }
        //    catch
        //    {
        //        trackAddedToPlaylist = null;
        //        return trackAddedToPlaylist ;
        //    }
        //}

        [HttpPost("createvote")]
        public async Task<GeneralPollInfoDTO> CreateVoteOnExistingPoll([Bind("CreateVotePlaylistId, CreateVoteUsername, CreateVoteOptionId")] SubmitVoteDTO userVote)
        {
            int numFollowersServerSide = 5; //Just pretending this is the actual follower count from spotify's end.

            GeneralPollInfoDTO ExistingPoll = new GeneralPollInfoDTO();


            //THIS CURRENTLY DOES NOT COVER SHOWING WHAT THE USER VOTED!
            if (numFollowersServerSide >= 5) 
            {
                ExistingPoll.TrackArtist = "PlaylistID: " + userVote.CreateVotePlaylistId;
                ExistingPoll.TrackTitle = "PRACTICE TRACK #100___Created_by_'removevote'";
                ExistingPoll.TrackDuration = "CUR_USER_IS: " + userVote.CreateVoteUsername;
                ExistingPoll.YesOptionID = "#1234_YES";
                ExistingPoll.NoOptionID = "#5678_NO";
                ExistingPoll.TotalPollVotes = "5";
                ExistingPoll.PlaylistFollowerCount = "5";
                ExistingPoll.YesVotes = 2;
                ExistingPoll.NoVotes = 3;

                return ExistingPoll;
            }
            else
            {
                ExistingPoll.UserVotedYes = false; //"False" displays correct value. "True" display correct value.//"null" display the correct value. "null" is meant for the "checkifpollexists" method. 
                ExistingPoll.TrackArtist = "PlaylistID: " + userVote.CreateVotePlaylistId;
                ExistingPoll.TrackTitle = "PRACTICE TRACK #100___Created_by_'removevote'";
                ExistingPoll.TrackDuration = "CUR_USER_IS: " + userVote.CreateVoteUsername;
                ExistingPoll.YesOptionID = "#1234_YES";
                ExistingPoll.NoOptionID = "#5678_NO";
                ExistingPoll.TotalPollVotes = "2";
                ExistingPoll.PlaylistFollowerCount = "5";

                return ExistingPoll;
            }
        }

        [HttpPost("removevote")]
        public async Task<GeneralPollInfoDTO> RemoveVoteOnExistingPoll([Bind("RemoveVotePlaylistID, RemoveVoteUsername")] RemoveVoteDTO removeVoteInput)
        {
            GeneralPollInfoDTO ExistingPoll = new GeneralPollInfoDTO();
            ExistingPoll.TrackArtist = "PlaylistID: " + removeVoteInput.RemoveVotePlaylistID;
            ExistingPoll.TrackTitle = "PRACTICE TRACK #100___Created_by_'removevote'";
            ExistingPoll.TrackDuration = "CUR_USER_IS: " + removeVoteInput.RemoveVoteUsername;
            ExistingPoll.YesOptionID = "#1234_YES";
            ExistingPoll.NoOptionID = "#5678_NO";
            ExistingPoll.TotalPollVotes = "4";
            ExistingPoll.PlaylistFollowerCount = "EMPTY_ON_PURPOSE";

            //bool successfulVoteRemoval = true;
            //try
            //{
            //    Poll PollInfo = _playlistPollRepository.GetPollDetailsBySpotifyPlaylistID(RemoveVotePlaylistID);
            //    VoteIdentifierInfoDTO CurUserVote = await _pollsService.GetSpecificUserVoteForAGivenPlaylist(PollInfo.PollId, RemoveVoteUsername);
            //    await _pollsService.RemoveVote(CurUserVote.VoteID);
            //    return successfulVoteRemoval;
            //}
            //catch(Exception ex) 
            //{
            //    successfulVoteRemoval = false;
            //    return successfulVoteRemoval;
            //}

            return ExistingPoll;

            /*
             * Need to return:
             * -Options
             */
        }
    }
}
