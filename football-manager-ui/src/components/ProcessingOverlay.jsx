import { Loader2 } from "lucide-react";

export default function ProcessingOverlay({
  logs,
  isProcessing,
  stopProcessing,
  allowCancel = true,
}) {
  if (!isProcessing) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50 pointer-events-auto"
      aria-modal="true"
      role="dialog"
    >
      <div className="bg-white rounded-2xl shadow-xl p-6 flex flex-col gap-4 w-[32rem] max-h-[80vh]">
        {/* Header */}
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <Loader2 className="animate-spin text-sky-600" size={28} />
            <p className="text-lg font-semibold text-slate-700">
              Processing...
            </p>
          </div>
          {allowCancel && (
            <button
              onClick={stopProcessing}
              className="text-sm px-3 py-1 rounded-md bg-red-50 hover:bg-red-100"
            >
              Cancel
            </button>
          )}
        </div>

        {/* Logs */}
        <div
          className="bg-slate-100 rounded p-3 text-sm text-slate-700 overflow-auto flex-1"
          aria-live="polite"
        >
          {logs.map((l, i) => {
            const matchRegex =
              /^(.+?): (.+?) (\d+) - (\d+) (.+?)(?: \(Your team\))?$/;
            const m = l.match(matchRegex);
            if (m) {
              const isUserMatch = l.includes("(Your team)");
              return (
                <div
                  key={i}
                  className={`mb-2 p-2 rounded shadow-sm ${
                    isUserMatch
                      ? "bg-yellow-100 border border-yellow-300"
                      : "bg-white"
                  }`}
                >
                  <div className="font-medium text-sky-700">{m[1]}</div>
                  <div className="flex justify-between font-semibold">
                    <span>{m[2]}</span>
                    <span>
                      {m[3]} - {m[4]}
                    </span>
                    <span>{m[5]}</span>
                  </div>
                  {isUserMatch && (
                    <div className="text-xs text-yellow-700 mt-1">
                      Your team match
                    </div>
                  )}
                </div>
              );
            }
            return (
              <div key={i} className="mb-1 whitespace-pre-wrap">
                {l}
              </div>
            );
          })}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between">
          <p className="text-xs text-slate-400">Please wait...</p>
        </div>
      </div>
    </div>
  );
}
