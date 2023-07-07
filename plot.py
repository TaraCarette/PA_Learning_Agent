from matplotlib import pyplot as plt
import tensorboard as tb

# this script generates graphs containing a selected value and puts different runs of that value on the same graph

# runs = [['layer3\\Tears', 'layer3_2\\Tears']]
# title = ['Wide Agent Layer 3 Forward Loss']
# statType = "Losses/Curiosity Forward Loss"

runs = [['run_loaded_1\\Tears', 'run_loaded_2\\Tears'], ['run_sparse_loaded_1\\Tears', 'run_sparse_loaded_2\\Tears'], ['run_wide_loaded_1\\Tears', 'run_wide_loaded_2\\Tears']]
# runs = [['run_1\\Tears', 'run_2\\Tears'], ['run_sparse_1\\Tears', 'run_sparse_2\\Tears'], ['run_wide_1\\Tears', 'run_wide_2\\Tears']]
title = ["Default Agent Layer 2 Future Loss", "Sparse Agent Layer 2 Future Loss", "Wide Agent Layer 2 Future Loss"]
statType = "Losses/Curiosity Future Loss"
scaleTested = False
save = False
ymin = -0.01
ymax = 0.01
ylabel = "Loss"


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
experiment_id = "fLceZyeAR4qjrxoy5yCNnw"
experiment = tb.data.experimental.ExperimentFromDev(experiment_id)
df = experiment.get_scalars()


legendLabels = ["Run 1", "Run 2"]


# each loop creates a new graph
for r in range(len(runs)):
	dfRunGroups = df.groupby(df["run"])


	ax = plt.gca()
	if scaleTested:
		ax.set_ylim([ymin, ymax])

	count = 0
	# this loop adds a new data line to the same graph
	for run in runs[r]:
		dfRun = dfRunGroups.get_group(run)

		dfRun = dfRun.groupby(df["tag"])
		dfRun = dfRun.get_group(statType)

		# # just for runs where need to annotate first value
		# firstValue = round(dfRun.head(1)["value"].item(), 3)
		# if count == 0:
		# 	ax.annotate(firstValue, xy=(30000, 0.01), xytext=(40000, 0.005), arrowprops={"arrowstyle":"->", "color":"blue"})
		# else:
		# 	ax.annotate(firstValue, xy=(30000, 0.01), xytext=(80000, 0.007), arrowprops={"arrowstyle":"->", "color":"orange"})
		# count += 1

		dfRun.plot(x = 'step', y = 'value', ax = ax)


	# create the properly formated graph
	plt.legend(legendLabels)
	plt.title(title[r])
	plt.ylabel(ylabel)
	plt.xlabel("Steps")

	fig = plt.gcf()
	plt.show()
	if save:
		fig.savefig("C:\\Users\\terra\\Desktop\\thesis\\figure pics\\graphs\\" + title[r])

