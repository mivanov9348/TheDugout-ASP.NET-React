import React, { useState } from "react";

const Finances = () => {
  const [balance, setBalance] = useState(1250000); // примерен баланс
  const [filter, setFilter] = useState("all");

  const transactions = [
    { id: 1, type: "income", description: "Ticket Sales", amount: 50000, date: "2025-09-01" },
    { id: 2, type: "expense", description: "Player Wages", amount: -120000, date: "2025-09-02" },
    { id: 3, type: "income", description: "Sponsorship", amount: 200000, date: "2025-09-03" },
    { id: 4, type: "expense", description: "Stadium Maintenance", amount: -50000, date: "2025-09-03" },
  ];

  const filteredTransactions =
    filter === "all" ? transactions : transactions.filter((t) => t.type === filter);

  return (
    <div className="p-6 space-y-6">
      {/* Баланс */}
      <div className="bg-white shadow-md rounded-2xl p-6 flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-700">Club Balance</h2>
        <p
          className={`text-2xl font-bold ${
            balance >= 0 ? "text-green-600" : "text-red-600"
          }`}
        >
          ${balance.toLocaleString()}
        </p>
      </div>

      {/* Филтър */}
      <div className="flex gap-4">
        <button
          onClick={() => setFilter("all")}
          className={`px-4 py-2 rounded-xl ${
            filter === "all"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          All
        </button>
        <button
          onClick={() => setFilter("income")}
          className={`px-4 py-2 rounded-xl ${
            filter === "income"
              ? "bg-green-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          Income
        </button>
        <button
          onClick={() => setFilter("expense")}
          className={`px-4 py-2 rounded-xl ${
            filter === "expense"
              ? "bg-red-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          Expenses
        </button>
      </div>

      {/* Таблица */}
      <div className="bg-white shadow-md rounded-2xl overflow-hidden">
        <table className="w-full table-auto">
          <thead className="bg-gray-100 text-gray-600 uppercase text-sm">
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
                className="border-b last:border-none hover:bg-gray-50 transition"
              >
                <td className="px-6 py-3">{t.date}</td>
                <td className="px-6 py-3">{t.description}</td>
                <td
                  className={`px-6 py-3 text-right font-medium ${
                    t.amount >= 0 ? "text-green-600" : "text-red-600"
                  }`}
                >
                  {t.amount >= 0 ? "+" : ""}
                  ${Math.abs(t.amount).toLocaleString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Finances;
