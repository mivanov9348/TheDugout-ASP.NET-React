import { useState, useEffect } from "react";

const Finances = ({ gameSaveId }) => {
  const [balance, setBalance] = useState(0);
  const [transactions, setTransactions] = useState([]);
  const [filter, setFilter] = useState("all");

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchFinance = async () => {
      try {
        const res = await fetch(`/api/finance/${gameSaveId}`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load finances");

        const data = await res.json();
        setBalance(data.balance);
        setTransactions(data.transactions);
      } catch (err) {
        console.error(err);
      }
    };

    fetchFinance();
  }, [gameSaveId]);

  const filteredTransactions =
    filter === "all"
      ? transactions
      : transactions.filter((t) =>
          filter === "income" ? t.amount > 0 : t.amount < 0
        );

  return (
    <div className="p-6 space-y-6 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 min-h-screen text-gray-100">
      {/* Баланс */}
      <div className="bg-gray-800/70 backdrop-blur-sm shadow-lg rounded-2xl p-6 flex justify-between items-center border border-gray-700">
        <h2 className="text-xl font-semibold text-gray-200">Club Balance</h2>
        <p
          className={`text-3xl font-bold ${
            balance >= 0 ? "text-green-400" : "text-red-400"
          }`}
        >
          ${balance.toLocaleString()}
        </p>
      </div>

      {/* Филтър */}
      <div className="flex gap-4">
        <button
          onClick={() => setFilter("all")}
          className={`px-4 py-2 rounded-xl font-medium transition ${
            filter === "all"
              ? "bg-blue-600 text-white shadow-md"
              : "bg-gray-700 hover:bg-gray-600 text-gray-300"
          }`}
        >
          All
        </button>
        <button
          onClick={() => setFilter("income")}
          className={`px-4 py-2 rounded-xl font-medium transition ${
            filter === "income"
              ? "bg-green-600 text-white shadow-md"
              : "bg-gray-700 hover:bg-gray-600 text-gray-300"
          }`}
        >
          Income
        </button>
        <button
          onClick={() => setFilter("expense")}
          className={`px-4 py-2 rounded-xl font-medium transition ${
            filter === "expense"
              ? "bg-red-600 text-white shadow-md"
              : "bg-gray-700 hover:bg-gray-600 text-gray-300"
          }`}
        >
          Expenses
        </button>
      </div>

      {/* Таблица */}
      <div className="bg-gray-800/70 backdrop-blur-sm shadow-lg rounded-2xl overflow-hidden border border-gray-700">
        <table className="w-full table-auto">
          <thead className="bg-gray-700 text-gray-300 uppercase text-sm">
            <tr>
              <th className="px-6 py-3 text-left">Date</th>
              <th className="px-6 py-3 text-left">Description</th>
              <th className="px-6 py-3 text-right">Amount</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.map((t) => (
              <tr
                key={t.id}
                className="border-b border-gray-700 last:border-none hover:bg-gray-700/50 transition"
              >
                <td className="px-6 py-3 text-gray-300">
                  {new Date(t.date).toLocaleDateString()}
                </td>
                <td className="px-6 py-3 text-gray-200">{t.description}</td>
                <td
                  className={`px-6 py-3 text-right font-semibold ${
                    t.amount >= 0 ? "text-green-400" : "text-red-400"
                  }`}
                >
                  {t.amount >= 0 ? "+" : "-"}$
                  {Math.abs(t.amount).toLocaleString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {filteredTransactions.length === 0 && (
          <div className="text-center py-8 text-gray-400">
            No transactions found.
          </div>
        )}
      </div>
    </div>
  );
};

export default Finances;
