import React, { useEffect, useRef } from "react";
import { Loader2, TerminalSquare, XCircle } from "lucide-react";

export default function ProcessingOverlay({
  logs,
  isProcessing,
  stopProcessing,
  allowCancel,
}) {
  const logEndRef = useRef(null);

  useEffect(() => {
    if (logEndRef.current) {
      logEndRef.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [logs]);

  if (!isProcessing) return null;

  return (
    <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 backdrop-blur-md">
      <div className="relative bg-gradient-to-br from-sky-700 via-sky-600 to-sky-800 w-full max-w-4xl rounded-2xl shadow-2xl overflow-hidden flex flex-col border border-sky-400/40">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-sky-900/60 border-b border-sky-500/40">
          <div className="flex items-center space-x-3 text-white">
            <Loader2 className="animate-spin h-6 w-6 text-cyan-300" />
            <h2 className="font-bold text-xl tracking-wide drop-shadow">
              Processing Matches...
            </h2>
          </div>
          <div className="flex items-center text-sm text-sky-200 italic">
            <TerminalSquare className="h-5 w-5 mr-1 text-sky-300" />
            Live Log Console
          </div>
        </div>

        {/* Logs section */}
        <div className="flex-1 bg-gray-900 text-gray-100 font-mono text-sm overflow-y-auto px-6 py-4 space-y-2 max-h-[600px] custom-scrollbar">
          {logs.length === 0 ? (
            <div className="text-gray-400 italic">Waiting for updates...</div>
          ) : (
            logs.map((line, idx) => (
              <div
                key={idx}
                className="whitespace-pre-wrap break-words transition-all duration-150 hover:text-sky-300"
              >
                {line}
              </div>
            ))
          )}
          <div ref={logEndRef} />
        </div>

        {/* Footer */}
        <div className="flex justify-between items-center px-6 py-3 bg-sky-900/50 border-t border-sky-500/40">
          <div className="text-xs text-sky-200">
            Showing last {logs.length > 500 ? "500+" : logs.length} lines
          </div>

          {allowCancel && (
            <button
              onClick={stopProcessing}
              className="flex items-center gap-2 px-4 py-2 rounded-xl bg-gradient-to-r from-red-500 to-red-600 text-white font-semibold shadow-lg hover:shadow-red-500/40 hover:scale-105 transition-all"
            >
              <XCircle className="w-5 h-5" />
              Cancel
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
