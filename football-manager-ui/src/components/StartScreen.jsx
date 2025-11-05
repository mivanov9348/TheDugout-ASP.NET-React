import React, { useState } from "react";
import Swal from "sweetalert2";
import { useNavigate } from "react-router-dom";
import { EventSourcePolyfill } from "event-source-polyfill";

function StartScreen({ username, onNewGame, onLoadGame, onLogout, isAdmin }) {
  const [loading, setLoading] = useState(false);
  const [loadingMessage, setLoadingMessage] = useState("");
  const navigate = useNavigate();

  const startNewGameStream = async () => {
    setLoading(true);

    Swal.fire({
      title: "Starting new game...",
      html: "<pre id='log-box' style='text-align:left;max-height:250px;overflow:auto;font-family:monospace;'></pre>",
      allowOutsideClick: false,
      showConfirmButton: false,
      didOpen: () => {
        const logBox = Swal.getHtmlContainer().querySelector("#log-box");

        const eventSource = new EventSourcePolyfill("/api/games/start-stream", {
          withCredentials: true,
        });

        eventSource.onmessage = (e) => {
          const p = document.createElement("div");
          p.textContent = e.data;
          logBox.appendChild(p);
          logBox.scrollTop = logBox.scrollHeight;
        };

        let createdSaveId = null;

        eventSource.addEventListener("result", (e) => {
          createdSaveId = e.data;
        });

        eventSource.addEventListener("done", async () => {
          eventSource.close();
          Swal.update({
            icon: "success",
            title: "✅ Game Created!",
            html: "<p>Loading team selection...</p>",
            showConfirmButton: false,
          });

          if (createdSaveId) {
            // подаваме на App id-то директно
            if (onNewGame) onNewGame({ id: createdSaveId });
          } else {
            Swal.fire("Error", "Game created, but no save ID received.", "error");
          }

          setLoading(false);
        });


        eventSource.onerror = (err) => {
          eventSource.close();
          setLoading(false);
          Swal.fire("Error", "Stream connection failed.", "error");
          console.error("SSE error", err);
        };
      },
    });
  };

  return (
    <div className="flex flex-col items-center justify-center h-screen bg-gradient-to-b from-gray-800 to-black text-white">
      <h1 className="text-5xl font-bold mb-6">⚽ The Dugout</h1>
      <p className="text-xl mb-10">
        Welcome, <span className="font-semibold">{username}</span>!
      </p>

      <div className="space-y-6 flex flex-col">
        <button
          onClick={startNewGameStream}
          className="px-8 py-3 bg-green-600 rounded-2xl hover:bg-green-500 transition"
        >
          New Game
        </button>
        <button
          onClick={onLoadGame}
          className="px-8 py-3 bg-blue-600 rounded-2xl hover:bg-blue-500 transition"
          disabled={loading}
        >
          Load Game
        </button>

        {isAdmin && (
          <button
            onClick={() => navigate("/admin")}
            className="px-8 py-3 bg-yellow-500 rounded-2xl hover:bg-yellow-400 transition"
          >
            Admin Panel
          </button>
        )}

        <button
          onClick={onLogout}
          className="px-8 py-3 bg-red-600 rounded-2xl hover:bg-red-500 transition"
          disabled={loading}
        >
          Logout
        </button>
      </div>
    </div>
  );
}

export default StartScreen;
