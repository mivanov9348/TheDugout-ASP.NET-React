import React from 'react';
import { useOutletContext } from "react-router-dom";
import TeamLogo from "../../../components/TeamLogo";
import { Link } from "react-router-dom";

export default function Knockouts() {
  const { selectedCup } = useOutletContext();

  return (

    <div className="bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 p-4 rounded-2xl shadow-lg text-gray-100">
      <h2 className="text-2xl font-bold mb-4 text-sky-400">Knockout Rounds</h2>

      {selectedCup?.rounds?.length === 0 ? (
        <p className="text-gray-400">No rounds yet.</p>
      ) : (
        selectedCup.rounds.map((round) => (
          <div key={round.id} className="mb-8">
            <h3 className="text-xl font-semibold mb-3 text-gray-200">{round.name}</h3>
            <div className="space-y-3">
              {round.fixtures.map((match) => (
                <div key={match.id} className="bg-gray-800 p-3 rounded-lg border border-gray-700">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2 flex-1">
                      <TeamLogo
                        teamName={match.homeTeam.name}
                        logoFileName={match.homeTeam.logoFileName}
                        className="w-8 h-8"
                      />
                      <span className="text-gray-200">{match.homeTeam.name}</span>
                    </div>

                    <span className="font-bold text-sky-400">
                      {match.status === "Played" && match.matchId ? (
                        <Link
                          to={`/match/${match.matchId}`}
                          className="hover:text-sky-300 transition-colors underline underline-offset-4"
                        >
                          {match.homeTeamGoals} - {match.awayTeamGoals}
                          {match.penaltiesResult && (
                            <span className="text-gray-500 text-sm">{match.penaltiesResult}</span>
                          )}
                        </Link>
                      ) : (
                        "—"
                      )}
                    </span>

                    <div className="flex items-center gap-2 flex-1 justify-end">
                      <span className="text-gray-200">{match.awayTeam.name}</span>
                      <TeamLogo
                        teamName={match.awayTeam.name}
                        logoFileName={match.awayTeam.logoFileName}
                        className="w-8 h-8"
                      />
                    </div>
                  </div>

                  {match.date && (
                    <div className="text-center text-sm text-gray-400 mt-2">
                      {new Date(match.date).toLocaleDateString("bg-BG", {
                        day: "2-digit",
                        month: "short",
                        year: "numeric",
                      })}
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>
        ))
      )}
    </div>

  );
}
