// src/components/ProcessingOverlay.jsx
import { Loader2 } from "lucide-react";

export default function ProcessingOverlay({ logs, isProcessing, stopProcessing, allowCancel = true }) {
  if (!isProcessing) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50 pointer-events-auto"
      aria-modal="true"
      role="dialog"
    >
      <div className="bg-white rounded-2xl shadow-xl p-6 flex flex-col gap-4 w-[32rem] max-h-[80vh]">
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <Loader2 className="animate-spin text-sky-600" size={28} />
            <p className="text-lg font-semibold text-slate-700">Processing...</p>
          </div>
          {allowCancel && (
            <button
              onClick={stopProcessing}
              className="text-sm px-3 py-1 rounded-md bg-slate-100 hover:bg-slate-200"
            >
              Close
            </button>
          )}
        </div>

        <div className="bg-slate-100 rounded p-3 text-sm text-slate-700 overflow-auto flex-1">
          {logs.map((l, i) => (
            <div key={i} className="mb-1 whitespace-pre-wrap">
              {l}
            </div>
          ))}
        </div>

        <div className="flex items-center justify-between">
          <p className="text-xs text-slate-400">Please wait...</p>
          {allowCancel && (
            <button
              onClick={stopProcessing}
              className="text-xs px-2 py-1 rounded bg-red-50 hover:bg-red-100"
            >
              Cancel
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
