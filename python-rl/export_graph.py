import tensorflow as tf
from tensorflow.python.tools import freeze_graph
import datetime
import os, argparse

def checkpoint_to_pb(checkpoint_path, pb_file):
    """Convert the entire checkpoint to a pb file"""

    meta_file = checkpoint_path + '.meta'
    if not os.path.isfile(meta_file):
        raise FileNotFoundError("Could not find checkpoint meta file.")
    # Start a session using a temporary fresh Graph
    with tf.Session(graph=tf.Graph()) as sess:
        # Import the meta graph in the current default Graph
        saver = tf.train.import_meta_graph(meta_file, clear_devices=True)
        # Restore the weights
        saver.restore(sess, checkpoint_path)
        head, tail = os.path.split(pb_file)
        tf.train.write_graph(tf.get_default_graph(), head, tail)

        print ("PB file saved to {}".format(pb_file))


def export_pb(model_name, output_node_names):
    """Create the bytes file that can be imported in Unity"""
    print('Creating the pb  file...')
    # Convert the checkpoint to pb file
    checkpoint_to_pb(model_name, model_name + '.pb')
    print('Freezing the graph in bytes file to be imported in Unity...')
    # Freeze the variables of the pb file and save the graph in a bytes file
    freeze_graph.freeze_graph(input_graph=model_name + '.pb', output_graph=model_name + '.bytes', input_checkpoint=model_name,
                          input_saver="", input_binary=False, restore_op_name="save/restore_all", filename_tensor_name=None,
                          clear_devices=True, initializer_nodes="",
                          output_node_names =output_node_names)
    print("Bytes file saved to {}".format(model_name + '.bytes'))

#export_pb(model_name='saved/boss_brain', output_node_names='ppo/actions-and-internals/categorical/sample/Select,ppo/actions-and-internals/layered-network/apply/internal_lstm0/apply/stack')
