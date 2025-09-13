import { useState } from "react";


const TeamLogo = ({ teamName, logoFileName, className = "" }) => {
  const [imgError, setImgError] = useState(false);

  const fileName = logoFileName || `${teamName}.png`;
  const logoPath = `/logos/${fileName}`;

  if (imgError || !fileName) {
    return (
      <div className={`w-8 h-8 bg-gray-300 rounded flex items-center justify-center text-xs font-bold ${className}`}>
        {teamName?.charAt(0).toUpperCase()}
      </div>
    );
  }

  return (
    <img
      src={logoPath}
      alt={`${teamName} logo`}
      className={`w-8 h-8 object-contain ${className}`}
      onError={() => setImgError(true)}
      loading="lazy"
    />
  );
};


export default TeamLogo;