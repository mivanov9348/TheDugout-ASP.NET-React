import { Navigate, Route } from "react-router-dom";

// --- Imports за всички страници ---
import Home from "../pages/Home";
import Inbox from "../pages/Inbox";
import Negotiations from "../pages/Negotiations";
import Transfers from "../pages/Transfers";
import TransferHistory from "../pages/players/TransferHistory";
import SearchPlayers from "../pages/players/SearchPlayers";
import PlayerProfile from "../pages/players/PlayerProfile";
import Club from "../pages/club/Club";
import Academy from "../pages/club/Academy";
import Finances from "../pages/club/Finances";
import Facilities from "../pages/club/Facilities";
import Squad from "../pages/club/Squad";
import Tactics from "../pages/club/Tactics";
import Training from "../pages/club/Training";
import Calendar from "../pages/season/Calendar";
import Fixtures from "../pages/season/Fixtures";
import SeasonReview from "../pages/season/SeasonReview";
import TodayMatches from "../pages/season/TodayMatches";
import Match from "../pages/season/Match";
import AllTimeStats from "../pages/AllTimeStats";
import Shortlist from "../pages/players/Shortlist";
import Settings from "../pages/Settings";

import Competitions from "../pages/competitions/Competitions";
import Cup from "../pages/competitions/Cup";
import EuropeanCup from "../pages/competitions/EuropeanCup";
import League from "../pages/competitions/League";

import LeagueStandings from "../pages/competitions/league/Standings";
import LeaguePlayerStats from "../pages/competitions/league/PlayerStats";

import CupPlayerStats from "../pages/competitions/cup/PlayerStats";
import CupKnockouts from "../pages/competitions/cup/Knockouts";

import EuropeGroupStage from "../pages/competitions/europe/GroupStage";
import EuropeKnockouts from "../pages/competitions/europe/Knockouts";
import EuropePlayerStats from "../pages/competitions/europe/PlayerStats";

export const AppRoutes = (gameSaveId, userTeamId, seasonId) => (
  <>
    <Route path="/" element={<Home gameSaveId={gameSaveId} />} />

    {/* Competitions */}
    <Route path="/competitions/*" element={<Competitions gameSaveId={gameSaveId} />}>
      <Route index element={<Navigate to="league" replace />} />

      {/* League */}
      <Route
        path="league/*"
        element={<League gameSaveId={gameSaveId} seasonId={seasonId} />}
      >
        <Route index element={<Navigate to="standings" replace />} />
        <Route path="standings" element={<LeagueStandings />} />
        <Route path="player-stats" element={<LeaguePlayerStats />} />
      </Route>

      {/* Cup */}
      <Route
        path="cup/*"
        element={<Cup gameSaveId={gameSaveId} seasonId={seasonId} />}
      >
        <Route index element={<Navigate to="knockouts" replace />} />
        <Route path="knockouts" element={<CupKnockouts />} />
        <Route path="player-stats" element={<CupPlayerStats />} />
      </Route>

      {/* European Cup */}
      <Route
        path="europe/*"
        element={<EuropeanCup gameSaveId={gameSaveId} seasonId={seasonId} />}
      >
        <Route index element={<Navigate to="groupstage" replace />} />
        <Route path="groupstage" element={<EuropeGroupStage />} />
        <Route path="knockouts" element={<EuropeKnockouts />} />
        <Route path="player-stats" element={<EuropePlayerStats />} />
      </Route>
    </Route>

    {/* Club & Team */}
    <Route path="/squad" element={<Squad gameSaveId={gameSaveId} />} />
    <Route
      path="/tactics"
      element={<Tactics gameSaveId={gameSaveId} teamId={userTeamId} />}
    />
    <Route
      path="/training"
      element={<Training gameSaveId={gameSaveId} teamId={userTeamId} />}
    />
    <Route
      path="/facilities"
      element={<Facilities gameSaveId={gameSaveId} teamId={userTeamId} />}
    />
    <Route path="/club" element={<Club gameSaveId={gameSaveId} />} />
    <Route path="/finances" element={<Finances gameSaveId={gameSaveId} />} />
    <Route path="/academy" element={<Academy teamId={userTeamId} />} />


    {/* Transfers */}
    <Route path="/transfers/*" element={<Transfers gameSaveId={gameSaveId} />}>
      <Route index element={<Navigate to="search" replace />} />
      <Route path="search" element={<SearchPlayers gameSaveId={gameSaveId} />} />
      <Route path="negotiations" element={<Negotiations gameSaveId={gameSaveId} />} />
      <Route path="history" element={<TransferHistory gameSaveId={gameSaveId} />} />
      <Route path="shortlist" element={<Shortlist gameSaveId={gameSaveId} />} />

    </Route>

    {/* Season */}
    <Route path="/calendar" element={<Calendar gameSaveId={gameSaveId} />} />
    <Route path="/fixtures" element={<Fixtures gameSaveId={gameSaveId} seasonId={seasonId} />} />
    <Route path="/season-review" element={<SeasonReview gameSaveId={gameSaveId} />} />
    <Route path="/today-matches/:gameSaveId" element={<TodayMatches />} />
    <Route path="/match/:matchId" element={<Match />} />
    <Route
      path="/all-time-stats"
      element={<AllTimeStats gameSaveId={gameSaveId} />}
    />

    {/* Player */}
    <Route path="/player/:playerId" element={<PlayerProfile gameSaveId={gameSaveId} />} />

    {/* Misc */}
    <Route path="/inbox" element={<Inbox gameSaveId={gameSaveId} />} />
    <Route path="/settings" element={<Settings />} />

    <Route path="*" element={<div>404 Not Found</div>} />
  </>
);
