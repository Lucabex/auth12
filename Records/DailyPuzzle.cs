using System.Text.Json.Serialization;


namespace auth12.Records;
public record DailyPuzzle(
    [property:JsonPropertyName("puzzle")] PuzzleInfo Puzzle
);

public record PuzzleInfo
(
    [property:JsonPropertyName("solution")]List<string> Solution,
    [property:JsonPropertyName("fen")]string Fen
);