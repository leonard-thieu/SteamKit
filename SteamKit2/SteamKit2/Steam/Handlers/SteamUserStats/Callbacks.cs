/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SteamKit2.Internal;

namespace SteamKit2
{
    public partial class SteamUserStats
    {
        /// <summary>
        /// This callback is fired in response to <see cref="GetNumberOfCurrentPlayers(uint)" />.
        /// </summary>
        public class NumberOfPlayersCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }
            /// <summary>
            /// Gets the current number of players according to Steam.
            /// </summary>
            public uint NumPlayers { get; private set; }


            internal NumberOfPlayersCallback( JobID jobID, CMsgDPGetNumberOfCurrentPlayersResponse resp )
            {
                this.JobID = jobID;
                this.Result = ( EResult )resp.eresult;
                this.NumPlayers = ( uint )resp.player_count;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="FindLeaderboard" /> and <see cref="CreateLeaderboard" />.
        /// </summary>
        public class FindOrCreateLeaderboardCallback : CallbackMsg, IFindOrCreateLeaderboardCallback
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }
            /// <summary>
            /// Leaderboard ID.
            /// </summary>
            public int ID { get; private set; }
            /// <summary>
            /// How many entires there are for requested leaderboard.
            /// </summary>
            public int EntryCount { get; private set; }
            /// <summary>
            /// Sort method to use for this leaderboard.
            /// </summary>
            public ELeaderboardSortMethod SortMethod { get; private set; }
            /// <summary>
            /// Display type for this leaderboard.
            /// </summary>
            public ELeaderboardDisplayType DisplayType { get; private set; }


            internal FindOrCreateLeaderboardCallback( JobID jobID, CMsgClientLBSFindOrCreateLBResponse resp )
            {
                this.JobID = jobID;

                this.Result = ( EResult )resp.eresult;
                this.ID = resp.leaderboard_id;
                this.EntryCount = resp.leaderboard_entry_count;
                this.SortMethod = ( ELeaderboardSortMethod )resp.leaderboard_sort_method;
                this.DisplayType = ( ELeaderboardDisplayType )resp.leaderboard_display_type;
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="FindLeaderboard" /> and <see cref="CreateLeaderboard" />.
        /// </summary>
        public interface IFindOrCreateLeaderboardCallback : ICallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            EResult Result { get; }
            /// <summary>
            /// Leaderboard ID.
            /// </summary>
            int ID { get; }
            /// <summary>
            /// How many entires there are for requested leaderboard.
            /// </summary>
            int EntryCount { get; }
            /// <summary>
            /// Sort method to use for this leaderboard.
            /// </summary>
            ELeaderboardSortMethod SortMethod { get; }
            /// <summary>
            /// Display type for this leaderboard.
            /// </summary>
            ELeaderboardDisplayType DisplayType { get; }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="GetLeaderboardEntries" />.
        /// </summary>
        public class LeaderboardEntriesCallback : CallbackMsg, ILeaderboardEntriesCallback
        {
            /// <summary>
            /// Represents a single package in this response.
            /// </summary>
            public sealed class LeaderboardEntry : ILeaderboardEntry
            {
                /// <summary>
                /// Gets the <see cref="SteamID"/> for this entry.
                /// </summary>
                public SteamID SteamID { get; private set; }
                /// <summary>
                /// Gets the global rank for this entry.
                /// </summary>
                public int GlobalRank { get; private set; }
                /// <summary>
                /// Gets the score for this entry.
                /// </summary>
                public int Score { get; private set; }
                /// <summary>
                /// Gets the <see cref="UGCHandle"/> attached to this entry.
                /// </summary>
                public UGCHandle UGCId { get; private set; }
                /// <summary>
                /// Extra game-defined information regarding how the user got that score.
                /// </summary>
                public ReadOnlyCollection<int> Details { get; private set; }


                internal LeaderboardEntry( CMsgClientLBSGetLBEntriesResponse.Entry entry )
                {
                    GlobalRank = entry.global_rank;
                    Score = entry.score;
                    SteamID = new SteamID( entry.steam_id_user );
                    UGCId = new UGCHandle( entry.ugc_id );

                    var details = new List<int>();

                    using ( var stream = new MemoryStream( entry.details ) )
                    {
                        while ( ( stream.Length - stream.Position ) >= sizeof( int ) )
                        {
                            details.Add( stream.ReadInt32() );
                        }
                    }

                    Details = new ReadOnlyCollection<int>( details );
                }
            }

            /// <summary>
            /// Represents a single package in this response.
            /// </summary>
            public interface ILeaderboardEntry
            {
                /// <summary>
                /// Gets the <see cref="SteamID"/> for this entry.
                /// </summary>
                SteamID SteamID { get; }
                /// <summary>
                /// Gets the global rank for this entry.
                /// </summary>
                int GlobalRank { get; }
                /// <summary>
                /// Gets the score for this entry.
                /// </summary>
                int Score { get; }
                /// <summary>
                /// Gets the <see cref="UGCHandle"/> attached to this entry.
                /// </summary>
                UGCHandle UGCId { get; }
                /// <summary>
                /// Extra game-defined information regarding how the user got that score.
                /// </summary>
                ReadOnlyCollection<int> Details { get; }
            }

            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            public EResult Result { get; private set; }
            /// <summary>
            /// How many entires there are for requested leaderboard.
            /// </summary>
            public int EntryCount { get; private set; }
            /// <summary>
            /// Gets the list of leaderboard entries this response contains.
            /// </summary>
            public ReadOnlyCollection<ILeaderboardEntry> Entries { get; private set; }


            internal LeaderboardEntriesCallback( JobID jobID, CMsgClientLBSGetLBEntriesResponse resp )
            {
                this.JobID = jobID;

                this.Result = ( EResult )resp.eresult;
                this.EntryCount = resp.leaderboard_entry_count;

                var list = new List<ILeaderboardEntry>();

                list.AddRange( resp.entries.Select( e => new LeaderboardEntry( e ) ) );

                Entries = new ReadOnlyCollection<ILeaderboardEntry>( list );
            }
        }

        /// <summary>
        /// This callback is fired in response to <see cref="GetLeaderboardEntries" />.
        /// </summary>
        public interface ILeaderboardEntriesCallback : ICallbackMsg
        {
            /// <summary>
            /// Gets the result of the request.
            /// </summary>
            EResult Result { get; }
            /// <summary>
            /// How many entires there are for requested leaderboard.
            /// </summary>
            int EntryCount { get; }
            /// <summary>
            /// Gets the list of leaderboard entries this response contains.
            /// </summary>
            ReadOnlyCollection<LeaderboardEntriesCallback.ILeaderboardEntry> Entries { get; }
        }
    }
}
