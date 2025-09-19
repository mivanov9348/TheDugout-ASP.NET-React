import { useState } from "react";

const PlayerAvatar = ({ playerName, imageFileName, className = "" }) => {
  const [imgError, setImgError] = useState(false);

  const fileName = imageFileName || `${playerName}.jpg`;
const imagePath = `avatars/${fileName}`;

  if (imgError || !fileName) {
    return (
      <div
        className={`w-10 h-10 bg-gray-300 rounded-full flex items-center justify-center text-sm font-bold ${className}`}
      >
        {playerName
          ?.split(" ")
          .map((n) => n.charAt(0).toUpperCase())
          .join("")}
      </div>
    );
  }

  return (
    <img
      src={imagePath}
      alt={`${playerName} avatar`}
      className={`w-10 h-10 rounded-full object-cover ${className}`}
      onError={() => setImgError(true)}
      loading="lazy"
    />
  );
};

export default PlayerAvatar;
