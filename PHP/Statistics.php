<?php

include "DatabaseConnection.php";

//top 5 scores of game 5

$query = "SELECT ROUND(AVG(Score)) AS gemiddelde, DATE_FORMAT(Scores.Datetime, '%m/%d/%Y') as create_date, COUNT(Scores.id) as amountofwins, u.Username FROM Scores LEFT JOIN Users u on (u.ID = User_ID) WHERE Scores.Datetime > DATE_ADD(NOW(), INTERVAL -1 MONTH) GROUP BY User_ID ORDER BY gemiddelde DESC LIMIT 10";

if (!($result = $mysqli->query($query))) {
    showerror($mysqli->errno,$mysqli->error);
}

$row = $result->fetch_assoc(); 
$alltimeRows = array();

do { 
    // echo json_encode($row) . "<br>";
    $alltimeRows[] = $row;
} while ($row = $result->fetch_assoc());
//  echo '{"allTime":' . json_encode( $alltimeRows) . '}';
 echo '{"topofmonth":' . json_encode( $alltimeRows) . '}';
// echo "<br>";
 
?> 