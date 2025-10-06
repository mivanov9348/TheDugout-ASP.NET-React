import { useOutletContext } from "react-router-dom";
import TeamLogo from "../../../components/TeamLogo";

export default function Knockouts() {
  const { selectedCup } = useOutletContext();

  return (
    <div>
      <h2 className="text-2xl font-bold mb-4">Knockout Rounds</h2>
      {selectedCup?.rounds?.length === 0 ? (
        <p className="text-gray-500">No rounds yet.</p>
      ) : (
        selectedCup.rounds.map((round) => (
          <div key={round.id} className="mb-8">
            <h3 className="text-xl font-semibold mb-3">{round.name}</h3>
            <div className="space-y-3">
              {round.fixtures.map((match) => (
                <div
                  key={match.id}
                  className="flex items-center justify-between bg-gray-50 p-3 rounded-lg border"
                >
                  <div className="flex items-center gap-2 flex-1">
                    <TeamLogo
                      teamName={match.homeTeam.name}
                      logoFileName={match.homeTeam.logoFileName}
                      className="w-8 h-8"
                    />
                    <span>{match.homeTeam.name}</span>
                  </div>
                  <span className="font-bold text-sky-600">
                    {match.status === 1
                      ? `${match.homeTeamGoals} - ${match.awayTeamGoals}`
                      : "—"}
                  </span>
                  <div className="flex items-center gap-2 flex-1 justify-end">
                    <span>{match.awayTeam.name}</span>
                    <TeamLogo
                      teamName={match.awayTeam.name}
                      logoFileName={match.awayTeam.logoFileName}
                      className="w-8 h-8"
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))
      )}
    </div>
  );
}
