from matplotlib import pyplot as plt
import tensorboard as tb

# the script generate graphs of different types of statistic, each containing the data from selected runs

runs = ['run_2\\Tears', 'run_sparse_2\\Tears', 'run_wide_2\\Tears']
# runs = ['run_loaded_2\\Tears', 'run_sparse_loaded_2\\Tears', 'run_wide_loaded_2\\Tears']


title = ["Comparing Agents Layer 2 Moving None", "Comparing Agents Layer 2 Moving Up", "Comparing Agents Layer 2 Moving Down", "Comparing Agents Layer 2 Moving Left", "Comparing Agents Layer 2 Moving Right"]
statType = ["Move/None", "Move/Up", "Move/Down", "Move/Left", "Move/Right"]
# title = ["Comparing Agents Layer 2 Rotating None", "Comparing Agents Layer 2 Rotating Right", "Comparing Agents Layer 2 Rotating Left"]
# statType = ["Rotate/None", "Rotate/Left", "Rotate/Right"]
# title = ["Comparing Agents Layer 2 Sticky Change", "Comparing Agents Layer 2 Sticky None Attached", "Comparing Agents Layer 2 Sticky One Attached", "Comparing Agents Layer 2 Sticky Multiple Attached"]
# statType = ["Sticky/Change", "Sticky/None Attached", "Sticky/One Attached", "Sticky/Multiple Attached"]
# title = ["Comparing Agents Layer 2 Touch Wall", "Comparing Agents Layer 2 Touch Object"]
# statType = ["Touch/Wall", "Touch/Object"]

save = False

# options
# ['Losses/Curiosity Forward Loss' 'Losses/Curiosity Inverse Loss'
#  'Losses/Policy Loss' 'Losses/Value Loss' 'Policy/Beta'
#  'Policy/Curiosity Value Estimate' 'Policy/Entropy' 'Policy/Epsilon'
#  'Policy/Learning Rate' 'Losses/Curiosity Future Loss' 'Reward/Curiosity'
#  'Location/X' 'Location/Y' 'Move/Down' 'Move/Left' 'Move/None'
#  'Move/Right' 'Move/Up' 'Rotate/Left' 'Rotate/None' 'Rotate/Right'
#  'Sticky/Change' 'Sticky/Multiple Attached' 'Sticky/No Change'
#  'Sticky/None Attached' 'Sticky/One Attached' 'Touch/Object' 'Touch/Wall']

# ['initial test future runs\\default_sparse\\Tears'
#  'initial test future runs\\future_360_1\\Tears'
#  'initial test future runs\\future_run_1\\Tears'
#  'initial test future runs\\future_run_layer_1\\Tears'
#  'initial test future runs\\future_run_layer_2\\Tears'
#  'initial test future runs\\future_run_sparse_1\\Tears'
#  'initial test future runs\\future_run_sparse_2\\Tears' 'run_1\\Tears'
#  'run_2\\Tears' 'run_loaded_1\\Tears' 'run_loaded_2\\Tears'
#  'run_sparse_1\\Tears' 'run_sparse_2\\Tears' 'run_sparse_loaded_1\\Tears'
#  'run_sparse_loaded_2\\Tears' 'run_wide_1\\Tears' 'run_wide_2\\Tears'
#  'run_wide_loaded_1\\Tears' 'run_wide_loaded_2\\Tears' 'test\\Tears']


# loading the data from the created tensorboard experiemnt
experiment_id = "whndVLarTZqgJmvEVCXNLg"
experiment = tb.data.experimental.ExperimentFromDev(experiment_id)
df = experiment.get_scalars()


legendLabels = ["Default", "Sparse", "Wide"]
ylabel = "Averaged Number of Actions"

# creates a graph for each stat type
for stat in range(len(statType)):
	dfRunGroups = df.groupby(df["run"])

	ax = plt.gca()

	# adds the statistic values of the specified runs to the graph
	for run in runs:
		dfRun = dfRunGroups.get_group(run)

		dfRun = dfRun.groupby(df["tag"])
		dfRun = dfRun.get_group(statType[stat])

		dfRun.plot(x = 'step', y = 'value', ax = ax)



	plt.legend(legendLabels)
	plt.title(title[stat])
	plt.ylabel(ylabel)
	plt.xlabel("Steps")

	fig = plt.gcf()
	plt.show()
	if save:
		fig.savefig("C:\\Users\\terra\\Desktop\\thesis\\figure pics\\graphs\\" + title[stat])

