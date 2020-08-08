<?php
include 'DatabaseConnection.php';

//Score and session_ID
SetExistingSession();
session_start();

if (!CheckServerID()) {
    echo "0";
    return;
}

$User_id = $_GET["User_ID"];
$Game_id = 1;
$Score = $_GET["Score"];


$query = "INSERT INTO Scores (id, User_ID, Game_ID, Score, Datetime) VALUES (NULL, $User_id, $Game_id, $Score, now())";

if (!($result = $mysqli->query($query))){
  showerror($mysqli->errno,$mysqli->error);
}

else {
	echo 1;
  }

?>
