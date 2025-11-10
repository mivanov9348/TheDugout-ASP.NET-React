import { useState, useEffect } from "react";
import { useGame } from "../context/GameContext";
import { useProcessing } from "../context/ProcessingContext";

export default function Settings() {
    const { currentGameSave } = useGame();
    const { startProcessing, stopProcessing, addLog } = useProcessing();
    const gameSaveId = currentGameSave?.id;

    const [fromDate, setFromDate] = useState("");
    const [toDate, setToDate] = useState("");

    useEffect(() => {
        if (!gameSaveId) return;
        const fetchCurrentSeason = async () => {
            const res = await fetch(`/api/season/current/${gameSaveId}`);
            if (res.ok) {
                const data = await res.json();
                setFromDate(data.currentDate.split("T")[0]);
            }
        };
        fetchCurrentSeason();
    }, [gameSaveId]);

    const handleSimulate = async () => {
        if (!gameSaveId) {
            alert("No active game save loaded.");
            return;
        }
        if (!toDate) {
            alert("Please select the end date before simulating.");
            return;
        }

        // 🟢 Показваме overlay-а
        startProcessing(`Simulating days until ${toDate}...`, { allowCancel: false });
        addLog("Starting daily simulation...");

        try {
            const res = await fetch(
                `/api/matches/simulate-to/${gameSaveId}?to=${toDate}`,
                { method: "POST" }
            );

            if (!res.ok) {
                const err = await res.text();
                addLog("Error: " + err);
                alert("Error: " + err);
                return;
            }

            const data = await res.json();
            addLog(`Simulation completed: ${data.totalSimulatedMatches} matches.`);
            alert(
                `Simulation finished: ${data.totalSimulatedMatches} matches simulated in ${data.totalDays} days.`
            );
        } catch (err) {
            console.error("Simulation error:", err);
            addLog("Simulation failed due to an error.");
            alert("An error occurred during simulation.");
        } finally {
            // 🟢 Скриваме overlay-а след края
            stopProcessing();
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 p-8 text-white">
            <h1 className="text-3xl font-bold mb-8 text-center text-sky-400">Settings</h1>

            <div className="max-w-md mx-auto bg-gray-800 rounded-2xl shadow-lg p-6 border border-gray-700">
                <h2 className="text-xl font-semibold mb-4 text-center text-sky-300">Simulate</h2>

                {!gameSaveId ? (
                    <p className="text-center text-gray-400">
                        No game save loaded. Please start or load a game first.
                    </p>
                ) : (
                    <div className="flex flex-col gap-4">
                        <div className="flex justify-between items-center">
                            <label htmlFor="fromDate" className="text-sm text-gray-300 w-20">
                                From:
                            </label>
                            <input
                                id="fromDate"
                                type="date"
                                value={fromDate}
                                onChange={(e) => setFromDate(e.target.value)}
                                className="flex-1 bg-gray-700 text-white px-3 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-sky-500"
                            />
                        </div>

                        <div className="flex justify-between items-center">
                            <label htmlFor="toDate" className="text-sm text-gray-300 w-20">
                                To:
                            </label>
                            <input
                                id="toDate"
                                type="date"
                                value={toDate}
                                onChange={(e) => setToDate(e.target.value)}
                                className="flex-1 bg-gray-700 text-white px-3 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-sky-500"
                            />
                        </div>

                        <button
                            onClick={handleSimulate}
                            className="mt-4 w-full bg-sky-600 hover:bg-sky-700 text-white font-bold py-2 rounded-lg transition"
                        >
                            Simulate
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}
