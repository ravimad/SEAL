$indir = $args[0]

mkdir $indir/stats
./StatsGenerator $indir/stats base $indir\base-stats.txt
./StatsGenerator $indir/stats plus-safe $indir\base-plus-safe-nodes-stats.txt
./StatsGenerator $indir/stats plus-sum-merge $indir\base-plus-summary-merging-stats.txt
./StatsGenerator $indir/stats plus-node-merge $indir\base-plus-node-merging-stats.txt
./StatsGenerator $indir/stats plus-ns-merge $indir\base-plus-node-summary-merging-stats.txt
./StatsGenerator $indir/stats plus-all $indir\base-plus-all-stats.txt